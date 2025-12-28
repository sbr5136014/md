using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var codeRichTextBox = new RichTextBox
            {
                FontFamily = new FontFamily("Consolas, Courier New, monospace"),
                FontSize = 14,
                Background = new SolidColorBrush(Color.FromRgb(0xf6, 0xf8, 0xfa)),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 16),
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.IBeam,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                IsDocumentEnabled = true
            };
            
            // Apply syntax highlighting with multiple colors
            ApplyAdvancedSyntaxHighlighting(codeRichTextBox, code);
            
            var border = new Border
            {
                Child = codeRichTextBox,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xe1, 0xe4, 0xe8)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromRgb(0xf6, 0xf8, 0xfa))
            };
            
            AddRichTextBoxContextMenu(codeRichTextBox);
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
        
        private void ApplyAdvancedSyntaxHighlighting(RichTextBox richTextBox, string code)
        {
            var doc = new FlowDocument();
            doc.PagePadding = new Thickness(0);
            doc.LineHeight = 1.2;
            
            // Detect language from code content
            string language = DetectCodeLanguage(code);
            
            // Apply token-based syntax highlighting based on language
            switch (language.ToLower())
            {
                case "csharp":
                case "c#":
                case "cs":
                    AddCSharpSyntaxHighlighting(doc, code);
                    break;
                case "javascript":
                case "js":
                    AddJavaScriptSyntaxHighlighting(doc, code);
                    break;
                case "python":
                case "py":
                    AddPythonSyntaxHighlighting(doc, code);
                    break;
                case "html":
                    AddHtmlSyntaxHighlighting(doc, code);
                    break;
                case "css":
                    AddCssSyntaxHighlighting(doc, code);
                    break;
                case "json":
                    AddJsonSyntaxHighlighting(doc, code);
                    break;
                default:
                    // Default styling
                    var paragraph = new Paragraph();
                    paragraph.Margin = new Thickness(0);
                    var run = new Run(code)
                    {
                        Foreground = new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2e))
                    };
                    paragraph.Inlines.Add(run);
                    doc.Blocks.Add(paragraph);
                    break;
            }
            
            richTextBox.Document = doc;
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
        
        // Syntax highlighting color scheme
        private readonly SolidColorBrush KeywordColor = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC)); // Blue
        private readonly SolidColorBrush StringColor = new SolidColorBrush(Color.FromRgb(0x00, 0x80, 0x00)); // Green
        private readonly SolidColorBrush CommentColor = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80)); // Gray
        private readonly SolidColorBrush NumberColor = new SolidColorBrush(Color.FromRgb(0xFF, 0x69, 0x00)); // Orange
        private readonly SolidColorBrush OperatorColor = new SolidColorBrush(Color.FromRgb(0x80, 0x00, 0x80)); // Purple
        private readonly SolidColorBrush TypeColor = new SolidColorBrush(Color.FromRgb(0x2B, 0x91, 0xAF)); // Teal
        private readonly SolidColorBrush DefaultColor = new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2E)); // Dark gray
        
        private void AddCSharpSyntaxHighlighting(FlowDocument doc, string code)
        {
            var csharpKeywords = new[] { "using", "namespace", "public", "private", "protected", "internal", 
                "class", "struct", "interface", "enum", "if", "else", "while", "for", "foreach", "do", "switch", 
                "case", "default", "break", "continue", "return", "try", "catch", "finally", "throw", "new", 
                "this", "base", "static", "readonly", "const", "var", "int", "string", "bool", "double", "float", 
                "decimal", "char", "byte", "long", "short", "uint", "ulong", "ushort", "object", "void" };
            
            ApplyGenericSyntaxHighlighting(doc, code, csharpKeywords, "//", "/*", "*/");
        }
        
        private void AddJavaScriptSyntaxHighlighting(FlowDocument doc, string code)
        {
            var jsKeywords = new[] { "function", "var", "let", "const", "if", "else", "while", "for", "do", 
                "switch", "case", "default", "break", "continue", "return", "try", "catch", "finally", "throw", 
                "new", "this", "typeof", "instanceof", "true", "false", "null", "undefined", "class", "extends", 
                "import", "export", "from", "async", "await", "Promise" };
            
            ApplyGenericSyntaxHighlighting(doc, code, jsKeywords, "//", "/*", "*/");
        }
        
        private void AddPythonSyntaxHighlighting(FlowDocument doc, string code)
        {
            var pythonKeywords = new[] { "def", "class", "if", "elif", "else", "while", "for", "try", "except", 
                "finally", "with", "as", "import", "from", "return", "yield", "break", "continue", "pass", 
                "lambda", "and", "or", "not", "in", "is", "True", "False", "None", "self", "print", "len", 
                "range", "str", "int", "float", "bool", "list", "dict", "tuple", "set" };
            
            ApplyGenericSyntaxHighlighting(doc, code, pythonKeywords, "#", "\"\"\"", "\"\"\"");
        }
        
        private void AddHtmlSyntaxHighlighting(FlowDocument doc, string code)
        {
            var paragraph = new Paragraph();
            paragraph.Margin = new Thickness(0);
            
            var lines = code.Split('\n');
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];
                var currentLine = line;
                
                while (!string.IsNullOrEmpty(currentLine))
                {
                    if (currentLine.StartsWith("<!--"))
                    {
                        // HTML comment
                        var endComment = currentLine.IndexOf("-->");
                        if (endComment != -1)
                        {
                            var comment = currentLine.Substring(0, endComment + 3);
                            paragraph.Inlines.Add(new Run(comment) { Foreground = CommentColor });
                            currentLine = currentLine.Substring(endComment + 3);
                        }
                        else
                        {
                            paragraph.Inlines.Add(new Run(currentLine) { Foreground = CommentColor });
                            currentLine = "";
                        }
                    }
                    else if (currentLine.StartsWith("<"))
                    {
                        // HTML tag
                        var endTag = currentLine.IndexOf(">");
                        if (endTag != -1)
                        {
                            var tag = currentLine.Substring(0, endTag + 1);
                            paragraph.Inlines.Add(new Run(tag) { Foreground = KeywordColor });
                            currentLine = currentLine.Substring(endTag + 1);
                        }
                        else
                        {
                            paragraph.Inlines.Add(new Run(currentLine) { Foreground = DefaultColor });
                            currentLine = "";
                        }
                    }
                    else
                    {
                        // Regular text
                        var nextTag = currentLine.IndexOf("<");
                        if (nextTag != -1)
                        {
                            var text = currentLine.Substring(0, nextTag);
                            paragraph.Inlines.Add(new Run(text) { Foreground = DefaultColor });
                            currentLine = currentLine.Substring(nextTag);
                        }
                        else
                        {
                            paragraph.Inlines.Add(new Run(currentLine) { Foreground = DefaultColor });
                            currentLine = "";
                        }
                    }
                }
                
                if (lineIndex < lines.Length - 1)
                {
                    paragraph.Inlines.Add(new LineBreak());
                }
            }
            
            doc.Blocks.Add(paragraph);
        }
        
        private void AddCssSyntaxHighlighting(FlowDocument doc, string code)
        {
            var cssKeywords = new[] { "color", "background", "font", "margin", "padding", "border", "width", 
                "height", "display", "position", "top", "left", "right", "bottom", "float", "clear", "text-align", 
                "font-size", "font-weight", "line-height", "text-decoration", "overflow", "z-index", "opacity" };
            
            ApplyGenericSyntaxHighlighting(doc, code, cssKeywords, "//", "/*", "*/");
        }
        
        private void AddJsonSyntaxHighlighting(FlowDocument doc, string code)
        {
            var paragraph = new Paragraph();
            paragraph.Margin = new Thickness(0);
            
            var lines = code.Split('\n');
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];
                var trimmedLine = line.Trim();
                
                if (trimmedLine.StartsWith("\"") && trimmedLine.Contains(":"))
                {
                    // Property name
                    var colonIndex = trimmedLine.IndexOf(":");
                    var propertyPart = trimmedLine.Substring(0, colonIndex + 1);
                    var valuePart = trimmedLine.Substring(colonIndex + 1);
                    
                    paragraph.Inlines.Add(new Run(propertyPart) { Foreground = KeywordColor });
                    paragraph.Inlines.Add(new Run(valuePart) { Foreground = StringColor });
                }
                else
                {
                    paragraph.Inlines.Add(new Run(line) { Foreground = DefaultColor });
                }
                
                if (lineIndex < lines.Length - 1)
                {
                    paragraph.Inlines.Add(new LineBreak());
                }
            }
            
            doc.Blocks.Add(paragraph);
        }
        
        private void ApplyGenericSyntaxHighlighting(FlowDocument doc, string code, string[] keywords, 
            string singleLineComment, string multiLineCommentStart, string multiLineCommentEnd)
        {
            var tokens = TokenizeCode(code);
            var paragraph = new Paragraph();
            paragraph.Margin = new Thickness(0);
            
            var currentText = "";
            SolidColorBrush currentColor = DefaultColor;
            
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                SolidColorBrush tokenColor = DefaultColor;
                
                if (token.Text == "\n")
                {
                    // Add any accumulated text before the line break
                    if (!string.IsNullOrEmpty(currentText))
                    {
                        paragraph.Inlines.Add(new Run(currentText) { Foreground = currentColor });
                        currentText = "";
                    }
                    paragraph.Inlines.Add(new LineBreak());
                    continue;
                }
                
                // Determine token color
                if (keywords.Any(k => k.Equals(token.Text, StringComparison.OrdinalIgnoreCase)))
                {
                    tokenColor = KeywordColor;
                }
                else if (token.Text.StartsWith("\"") && token.Text.EndsWith("\""))
                {
                    tokenColor = StringColor;
                }
                else if (token.Text.StartsWith("'") && token.Text.EndsWith("'"))
                {
                    tokenColor = StringColor;
                }
                else if (token.Text.StartsWith(singleLineComment))
                {
                    tokenColor = CommentColor;
                }
                else if (token.Text.StartsWith(multiLineCommentStart))
                {
                    tokenColor = CommentColor;
                }
                else if (Regex.IsMatch(token.Text, @"^\d+\.?\d*$"))
                {
                    tokenColor = NumberColor;
                }
                else if (token.Text.Length == 1 && "+-*/=<>!&|".Contains(token.Text))
                {
                    tokenColor = OperatorColor;
                }
                else
                {
                    tokenColor = DefaultColor;
                }
                
                // If color changed, flush current text and start new one
                if (currentColor != tokenColor)
                {
                    if (!string.IsNullOrEmpty(currentText))
                    {
                        paragraph.Inlines.Add(new Run(currentText) { Foreground = currentColor });
                        currentText = "";
                    }
                    currentColor = tokenColor;
                }
                
                currentText += token.Text;
            }
            
            // Add any remaining text
            if (!string.IsNullOrEmpty(currentText))
            {
                paragraph.Inlines.Add(new Run(currentText) { Foreground = currentColor });
            }
            
            doc.Blocks.Add(paragraph);
        }
        
        private List<CodeToken> TokenizeCode(string code)
        {
            var tokens = new List<CodeToken>();
            var currentToken = "";
            bool inString = false;
            char stringChar = '"';
            bool inComment = false;
            bool inSingleLineComment = false;
            
            for (int i = 0; i < code.Length; i++)
            {
                char c = code[i];
                
                // Handle string literals
                if (!inComment && !inSingleLineComment && (c == '"' || c == '\''))
                {
                    if (!inString)
                    {
                        if (!string.IsNullOrEmpty(currentToken))
                        {
                            tokens.Add(new CodeToken { Text = currentToken });
                            currentToken = "";
                        }
                        inString = true;
                        stringChar = c;
                        currentToken += c;
                    }
                    else if (c == stringChar && (i == 0 || code[i - 1] != '\\'))
                    {
                        currentToken += c;
                        tokens.Add(new CodeToken { Text = currentToken });
                        currentToken = "";
                        inString = false;
                    }
                    else
                    {
                        currentToken += c;
                    }
                    continue;
                }
                
                if (inString)
                {
                    currentToken += c;
                    continue;
                }
                
                // Handle comments
                if (!inString && i < code.Length - 1)
                {
                    if (code[i] == '/' && code[i + 1] == '/')
                    {
                        if (!string.IsNullOrEmpty(currentToken))
                        {
                            tokens.Add(new CodeToken { Text = currentToken });
                            currentToken = "";
                        }
                        inSingleLineComment = true;
                        currentToken += "//";
                        i++;
                        continue;
                    }
                    else if (code[i] == '/' && code[i + 1] == '*')
                    {
                        if (!string.IsNullOrEmpty(currentToken))
                        {
                            tokens.Add(new CodeToken { Text = currentToken });
                            currentToken = "";
                        }
                        inComment = true;
                        currentToken += "/*";
                        i++;
                        continue;
                    }
                }
                
                if (inSingleLineComment && c == '\n')
                {
                    tokens.Add(new CodeToken { Text = currentToken });
                    currentToken = "";
                    inSingleLineComment = false;
                    tokens.Add(new CodeToken { Text = "\n" });
                    continue;
                }
                
                if (inComment && i < code.Length - 1 && code[i] == '*' && code[i + 1] == '/')
                {
                    currentToken += "*/";
                    tokens.Add(new CodeToken { Text = currentToken });
                    currentToken = "";
                    inComment = false;
                    i++;
                    continue;
                }
                
                if (inComment || inSingleLineComment)
                {
                    currentToken += c;
                    continue;
                }
                
                // Handle operators and separators
                if ("+-*/=<>!&|(){}[];,.:".Contains(c))
                {
                    if (!string.IsNullOrEmpty(currentToken))
                    {
                        tokens.Add(new CodeToken { Text = currentToken });
                        currentToken = "";
                    }
                    tokens.Add(new CodeToken { Text = c.ToString() });
                }
                else if (c == '\n')
                {
                    if (!string.IsNullOrEmpty(currentToken))
                    {
                        tokens.Add(new CodeToken { Text = currentToken });
                        currentToken = "";
                    }
                    tokens.Add(new CodeToken { Text = "\n" });
                }
                else if (c == ' ' || c == '\t')
                {
                    if (!string.IsNullOrEmpty(currentToken))
                    {
                        tokens.Add(new CodeToken { Text = currentToken });
                        currentToken = "";
                    }
                    
                    // Collect consecutive whitespace into single token
                    string whitespace = "";
                    while (i < code.Length && (code[i] == ' ' || code[i] == '\t'))
                    {
                        whitespace += code[i];
                        i++;
                    }
                    i--; // Adjust for the loop increment
                    tokens.Add(new CodeToken { Text = whitespace });
                }
                else
                {
                    currentToken += c;
                }
            }
            
            if (!string.IsNullOrEmpty(currentToken))
            {
                tokens.Add(new CodeToken { Text = currentToken });
            }
            
            return tokens;
        }
        
        private class CodeToken
        {
            public string Text { get; set; }
        }
        
        private void AddRichTextBoxContextMenu(RichTextBox richTextBox)
        {
            var contextMenu = new ContextMenu();
            
            var copyItem = new MenuItem
            {
                Header = "Copy",
                Icon = new TextBlock { Text = "📋", FontFamily = new FontFamily("Segoe UI Emoji") }
            };
            copyItem.Click += (s, e) => CopyRichTextBoxContent(richTextBox);
            
            var selectAllItem = new MenuItem
            {
                Header = "Select All",
                Icon = new TextBlock { Text = "🔘", FontFamily = new FontFamily("Segoe UI Emoji") }
            };
            selectAllItem.Click += (s, e) => richTextBox.SelectAll();
            
            contextMenu.Items.Add(copyItem);
            contextMenu.Items.Add(selectAllItem);
            
            richTextBox.ContextMenu = contextMenu;
        }
        
        private void CopyRichTextBoxContent(RichTextBox richTextBox)
        {
            if (!richTextBox.Selection.IsEmpty)
            {
                Clipboard.SetText(richTextBox.Selection.Text);
                statusText.Text = "Selected text copied to clipboard";
            }
            else
            {
                var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                Clipboard.SetText(textRange.Text);
                statusText.Text = "All code copied to clipboard";
            }
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