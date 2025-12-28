# Code Highlighting Test

This document tests syntax highlighting for different programming languages.

## C# Code

```csharp
using System;

namespace HelloWorld
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
```

## JavaScript Code

```javascript
function greetUser(name) {
    const greeting = `Hello, ${name}!`;
    console.log(greeting);
    return greeting;
}

let userName = "SmartArt";
greetUser(userName);
```

## Python Code

```python
def calculate_fibonacci(n):
    if n <= 1:
        return n
    return calculate_fibonacci(n-1) + calculate_fibonacci(n-2)

# Print first 10 Fibonacci numbers
for i in range(10):
    print(f"Fib({i}) = {calculate_fibonacci(i)}")
```

## HTML Code

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Sample Page</title>
</head>
<body>
    <h1>Welcome to SmartArt Tech</h1>
    <div class="container">
        <p>This is a sample HTML document.</p>
    </div>
</body>
</html>
```

## CSS Code

```css
body {
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    margin: 0;
    padding: 20px;
    background-color: #f5f5f5;
}

.container {
    max-width: 800px;
    margin: 0 auto;
    background: white;
    padding: 20px;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}
```

## JSON Data

```json
{
    "name": "SmartArt Tech",
    "products": [
        {
            "name": "Markdown Viewer",
            "version": "1.0.0",
            "features": [
                "Native WPF rendering",
                "Syntax highlighting",
                "Text selection and copy"
            ]
        }
    ],
    "contact": {
        "website": "https://smartart.tech",
        "email": "info@smartart.tech"
    }
}
```

## Features to Test

- **Text Selection**: Try selecting any text in this document
- **Copy Function**: Right-click and select "Copy" or use Ctrl+C
- **Select All**: Right-click and select "Select All" or use Ctrl+A
- **Code Colors**: Each code block should have different colors based on language
- **Context Menu**: Right-click any text for copy options