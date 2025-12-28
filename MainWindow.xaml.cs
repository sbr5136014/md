using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace MarkdownViewer
{
    public partial class MainWindow : Window
    {
        private string currentFilePath = "";
        
        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
            
            // Add keyboard shortcuts
            this.KeyDown += MainWindow_KeyDown;
        }

        private void InitializeAsync()
        {
            // Check if a file was passed as command line argument
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && File.Exists(args[1]) && args[1].EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                OpenMarkdownFile(args[1]);
            }
            else
            {
                ShowWelcomeMessage();
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
            {
                OpenFile_Click(sender, null);
            }
            else if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Copy functionality is handled by individual TextBox controls
                // This ensures Ctrl+C works globally
                var focusedElement = Keyboard.FocusedElement as TextBox;
                if (focusedElement != null)
                {
                    CopySelectedText(focusedElement);
                }
            }
            else if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Select All functionality
                var focusedElement = Keyboard.FocusedElement as TextBox;
                if (focusedElement != null)
                {
                    focusedElement.SelectAll();
                }
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Markdown files (*.md)|*.md|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                OpenMarkdownFile(openFileDialog.FileName);
            }
        }

        private void OpenMarkdownFile(string filePath)
        {
            try
            {
                string markdownContent = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(markdownContent))
                {
                    statusText.Text = "File is empty";
                    MessageBox.Show("The selected file appears to be empty.", "Info", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                // Render markdown using native WPF controls
                RenderMarkdownToWpf(markdownContent);
                
                currentFilePath = filePath;
                filePathText.Text = filePath;
                statusText.Text = $"Loaded: {Path.GetFileName(filePath)} ({markdownContent.Length} chars)";
                Title = $"Markdown Viewer - {Path.GetFileName(filePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                statusText.Text = "Error loading file";
            }
        }

        private void RenderMarkdownToWpf(string markdown)
        {
            contentPanel.Children.Clear();
            
            if (string.IsNullOrWhiteSpace(markdown))
            {
                var emptyText = new TextBlock 
                { 
                    Text = "No content to display", 
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 10, 0, 10)
                };
                contentPanel.Children.Add(emptyText);
                return;
            }
            
            var lines = markdown.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            bool inCodeBlock = false;
            var codeBlockContent = new List<string>();
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmedLine = line.Trim();
                
                // Handle code blocks
                if (trimmedLine.StartsWith("```"))
                {
                    if (inCodeBlock)
                    {
                        // End code block
                        CreateCodeBlock(string.Join("\n", codeBlockContent));
                        codeBlockContent.Clear();
                        inCodeBlock = false;
                    }
                    else
                    {
                        // Start code block
                        inCodeBlock = true;
                    }
                    continue;
                }
                
                if (inCodeBlock)
                {
                    codeBlockContent.Add(line);
                    continue;
                }
                
                // Skip empty lines (they add natural spacing)
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                
                // Headers
                if (trimmedLine.StartsWith("#"))
                {
                    CreateHeader(trimmedLine);
                }
                // Lists
                else if (Regex.IsMatch(trimmedLine, @"^[-\*\+]\s+") || Regex.IsMatch(trimmedLine, @"^\d+\.\s+"))
                {
                    CreateListItem(trimmedLine);
                }
                // Blockquotes
                else if (trimmedLine.StartsWith(">"))
                {
                    CreateBlockquote(trimmedLine.Substring(1).Trim());
                }
                // Horizontal rules
                else if (Regex.IsMatch(trimmedLine, @"^---+$"))
                {
                    CreateHorizontalRule();
                }
                // Regular paragraphs
                else
                {
                    CreateParagraph(line);
                }
            }
            
            // Handle any remaining code block
            if (inCodeBlock && codeBlockContent.Count > 0)
            {
                CreateCodeBlock(string.Join("\n", codeBlockContent));
            }
        }
        
        private void CreateHeader(string headerLine)
        {
            int level = 0;
            while (level < headerLine.Length && headerLine[level] == '#')
                level++;
            
            string text = headerLine.Substring(level).Trim();
            
            var headerBox = new TextBox
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, level == 1 ? 20 : 15, 0, 10),
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                TextWrapping = TextWrapping.Wrap,
                Cursor = Cursors.IBeam
            };
            
            switch (level)
            {
                case 1:
                    headerBox.FontSize = 32;
                    headerBox.Foreground = new SolidColorBrush(Color.FromRgb(0x1f, 0x23, 0x28));
                    break;
                case 2:
                    headerBox.FontSize = 24;
                    headerBox.Foreground = new SolidColorBrush(Color.FromRgb(0x1f, 0x23, 0x28));
                    break;
                case 3:
                    headerBox.FontSize = 20;
                    headerBox.Foreground = new SolidColorBrush(Color.FromRgb(0x1f, 0x23, 0x28));
                    break;
                default:
                    headerBox.FontSize = 16;
                    headerBox.Foreground = new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2e));
                    break;
            }
            
            AddContextMenu(headerBox);
            contentPanel.Children.Add(headerBox);
        }
        
        private void CreateParagraph(string text)
        {
            var paragraph = new TextBox
            {
                Text = ProcessInlineFormatting(text),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 16),
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2e)),
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Cursor = Cursors.IBeam
            };
            
            AddContextMenu(paragraph);
            contentPanel.Children.Add(paragraph);
        }
        
        private void CreateListItem(string listItem)
        {
            string bullet = "• ";
            string text = "";
            
            if (Regex.IsMatch(listItem.Trim(), @"^[-\*\+]\s+"))
            {
                text = Regex.Replace(listItem.Trim(), @"^[-\*\+]\s+(.+)", "$1");
            }
            else if (Regex.IsMatch(listItem.Trim(), @"^\d+\.\s+"))
            {
                var match = Regex.Match(listItem.Trim(), @"^(\d+)\.\s+(.+)");
                bullet = match.Groups[1].Value + ". ";
                text = match.Groups[2].Value;
            }
            
            var listPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(20, 0, 0, 8)
            };
            
            var bulletBlock = new TextBlock
            {
                Text = bullet,
                FontSize = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2e))
            };
            
            var textBox = new TextBox
            {
                Text = ProcessInlineFormatting(text),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2e)),
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Cursor = Cursors.IBeam
            };
            
            AddContextMenu(textBox);
            listPanel.Children.Add(bulletBlock);
            listPanel.Children.Add(textBox);
            contentPanel.Children.Add(listPanel);
        }
        
        private void CreateCodeBlock(string code)
        {
            var codeBox = new TextBox
            {
                Text = code,
                FontFamily = new FontFamily("Consolas, Courier New, monospace"),
                FontSize = 14,
                Background = new SolidColorBrush(Color.FromRgb(0xf6, 0xf8, 0xfa)),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 16),
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2e)),
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.IBeam,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            
            // Apply syntax highlighting
            ApplySyntaxHighlighting(codeBox, code);
            
            var border = new Border
            {
                Child = codeBox,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xe1, 0xe4, 0xe8)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromRgb(0xf6, 0xf8, 0xfa))
            };
            
            AddContextMenu(codeBox);
            contentPanel.Children.Add(border);
        }
        
        private void CreateBlockquote(string text)
        {
            var quoteBox = new TextBox
            {
                Text = ProcessInlineFormatting(text),
                FontSize = 16,
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(0x6a, 0x73, 0x7d)),
                Margin = new Thickness(16, 0, 0, 16),
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Cursor = Cursors.IBeam
            };
            
            var border = new Border
            {
                Child = quoteBox,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xdf, 0xe2, 0xe5)),
                BorderThickness = new Thickness(4, 0, 0, 0),
                Padding = new Thickness(16, 0, 0, 0)
            };
            
            AddContextMenu(quoteBox);
            contentPanel.Children.Add(border);
        }
        
        private void CreateHorizontalRule()
        {
            var rule = new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(0xea, 0xec, 0xef)),
                Margin = new Thickness(0, 24, 0, 24)
            };
            
            contentPanel.Children.Add(rule);
        }
        
        private string ProcessInlineFormatting(string text)
        {
            // For now, just return the text as-is
            // In a more advanced implementation, we could use Run elements for formatting
            
            // Remove basic markdown formatting for display
            text = Regex.Replace(text, @"\*\*(.+?)\*\*", "$1"); // Bold
            text = Regex.Replace(text, @"\*(.+?)\*", "$1");     // Italic
            text = Regex.Replace(text, @"`(.+?)`", "$1");       // Inline code
            text = Regex.Replace(text, @"\[(.+?)\]\(.+?\)", "$1"); // Links
            
            return text;
        }
        
        private void AddContextMenu(TextBox textBox)
        {
            var contextMenu = new ContextMenu();
            
            var copyItem = new MenuItem
            {
                Header = "Copy",
                Icon = new TextBlock { Text = "📋", FontFamily = new FontFamily("Segoe UI Emoji") }
            };
            copyItem.Click += (s, e) => CopySelectedText(textBox);
            
            var selectAllItem = new MenuItem
            {
                Header = "Select All",
                Icon = new TextBlock { Text = "🔘", FontFamily = new FontFamily("Segoe UI Emoji") }
            };
            selectAllItem.Click += (s, e) => textBox.SelectAll();
            
            contextMenu.Items.Add(copyItem);
            contextMenu.Items.Add(selectAllItem);
            
            textBox.ContextMenu = contextMenu;
        }
        
        private void CopySelectedText(TextBox textBox)
        {
            if (!string.IsNullOrEmpty(textBox.SelectedText))
            {
                Clipboard.SetText(textBox.SelectedText);
                statusText.Text = "Text copied to clipboard";
            }
            else if (!string.IsNullOrEmpty(textBox.Text))
            {
                Clipboard.SetText(textBox.Text);
                statusText.Text = "All text copied to clipboard";
            }
        }
        
        private void ApplySyntaxHighlighting(TextBox codeBox, string code)
        {
            // Detect language from code content
            string language = DetectCodeLanguage(code);
            
            // Apply basic syntax highlighting based on language
            switch (language.ToLower())
            {
                case "csharp":
                case "c#":
                case "cs":
                    ApplyCSharpHighlighting(codeBox);
                    break;
                case "javascript":
                case "js":
                    ApplyJavaScriptHighlighting(codeBox);
                    break;
                case "python":
                case "py":
                    ApplyPythonHighlighting(codeBox);
                    break;
                case "html":
                    ApplyHtmlHighlighting(codeBox);
                    break;
                case "css":
                    ApplyCssHighlighting(codeBox);
                    break;
                case "json":
                    ApplyJsonHighlighting(codeBox);
                    break;
                default:
                    // Default monospace styling
                    codeBox.Foreground = new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2e));
                    break;
            }
        }
        
        private string DetectCodeLanguage(string code)
        {
            code = code.ToLower().Trim();
            
            // C# detection
            if (code.Contains("using system") || code.Contains("public class") || 
                code.Contains("namespace") || code.Contains("console.writeline"))
                return "csharp";
            
            // JavaScript detection
            if (code.Contains("function ") || code.Contains("const ") || 
                code.Contains("let ") || code.Contains("console.log"))
                return "javascript";
            
            // Python detection
            if (code.Contains("def ") || code.Contains("import ") || 
                code.Contains("print(") || code.Contains("if __name__"))
                return "python";
            
            // HTML detection
            if (code.Contains("<!doctype") || code.Contains("<html") || 
                code.Contains("<div") || code.Contains("</"))
                return "html";
            
            // CSS detection
            if (code.Contains("{") && code.Contains("}") && 
                (code.Contains("color:") || code.Contains("font-") || code.Contains("margin")))
                return "css";
            
            // JSON detection
            if ((code.StartsWith("{") && code.EndsWith("}")) || 
                (code.StartsWith("[") && code.EndsWith("]")))
                return "json";
            
            return "text";
        }
        
        private void ApplyCSharpHighlighting(TextBox codeBox)
        {
            // For now, just use different color for C# code
            codeBox.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x73, 0x99)); // Blue for C#
        }
        
        private void ApplyJavaScriptHighlighting(TextBox codeBox)
        {
            codeBox.Foreground = new SolidColorBrush(Color.FromRgb(0xf7, 0xdf, 0x1e)); // JavaScript yellow
        }
        
        private void ApplyPythonHighlighting(TextBox codeBox)
        {
            codeBox.Foreground = new SolidColorBrush(Color.FromRgb(0x35, 0x73, 0xa8)); // Python blue
        }
        
        private void ApplyHtmlHighlighting(TextBox codeBox)
        {
            codeBox.Foreground = new SolidColorBrush(Color.FromRgb(0xe3, 0x4c, 0x26)); // HTML orange
        }
        
        private void ApplyCssHighlighting(TextBox codeBox)
        {
            codeBox.Foreground = new SolidColorBrush(Color.FromRgb(0x15, 0x72, 0xb6)); // CSS blue
        }
        
        private void ApplyJsonHighlighting(TextBox codeBox)
        {
            codeBox.Foreground = new SolidColorBrush(Color.FromRgb(0x8e, 0x44, 0xad)); // JSON purple
        }

        private void ShowWelcomeMessage()
        {
            contentPanel.Children.Clear();
            
            // Create welcome content using WPF controls
            var titleBlock = new TextBlock
            {
                Text = "📝 Markdown Viewer",
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 40, 0, 20),
                Foreground = new SolidColorBrush(Color.FromRgb(0x1f, 0x23, 0x28))
            };
            
            var subtitleBlock = new TextBlock
            {
                Text = "Welcome to the simple Markdown file viewer!",
                FontSize = 18,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2e))
            };
            
            var instructionBlock = new TextBlock
            {
                Text = "Open a Markdown (.md) file to get started.",
                FontSize = 16,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 40),
                Foreground = new SolidColorBrush(Color.FromRgb(0x6a, 0x73, 0x7d))
            };
            
            var howToTitle = new TextBlock
            {
                Text = "How to use:",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 12),
                Foreground = new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2e))
            };
            
            var instructions = new string[]
            {
                "• Click File → Open or press Ctrl+O",
                "• Drag and drop .md files (when set as default handler)",
                "• Use Settings → Set as Default MD Viewer to handle .md files"
            };
            
            contentPanel.Children.Add(titleBlock);
            contentPanel.Children.Add(subtitleBlock);
            contentPanel.Children.Add(instructionBlock);
            contentPanel.Children.Add(howToTitle);
            
            foreach (var instruction in instructions)
            {
                var instructBlock = new TextBlock
                {
                    Text = instruction,
                    FontSize = 14,
                    Margin = new Thickness(20, 0, 0, 8),
                    Foreground = new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2e))
                };
                contentPanel.Children.Add(instructBlock);
            }
            
            var brandingBlock = new TextBlock
            {
                Text = "Developed by SmartArt Tech",
                FontSize = 12,
                FontStyle = FontStyles.Italic,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 40, 0, 20),
                Foreground = new SolidColorBrush(Color.FromRgb(0x6a, 0x73, 0x7d))
            };
            
            contentPanel.Children.Add(brandingBlock);
            statusText.Text = "Ready - Open a Markdown file to begin";
        }


        private void SetAsDefault_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegisterAsDefaultHandler();
                MessageBox.Show("Markdown Viewer has been set as the default handler for .md files.", 
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                statusText.Text = "Set as default .md file handler";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting as default handler: {ex.Message}\n\nPlease run as administrator.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                statusText.Text = "Failed to set as default handler";
            }
        }

        private void RegisterAsDefaultHandler()
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (exePath.EndsWith(".dll"))
            {
                exePath = exePath.Replace(".dll", ".exe");
            }

            // Register file association for .md files
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(".md"))
            {
                key.SetValue("", "MarkdownViewer.Document");
            }

            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey("MarkdownViewer.Document"))
            {
                key.SetValue("", "Markdown Document");
                
                using (RegistryKey iconKey = key.CreateSubKey("DefaultIcon"))
                {
                    iconKey.SetValue("", $"{exePath},0");
                }
                
                using (RegistryKey commandKey = key.CreateSubKey("shell\\open\\command"))
                {
                    commandKey.SetValue("", $"\"{exePath}\" \"%1\"");
                }
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Markdown Viewer v1.0\n\nA simple Windows application for viewing Markdown files.\n\nBuilt with WPF and custom markdown parser.\n\nDeveloped by SmartArt Tech\n© 2024 SmartArt Tech. All rights reserved.", 
                "About Markdown Viewer", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}