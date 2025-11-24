# Book Cover Upload Guide

## How to Add Book Covers to BookHub

### Step 1: Navigate to Images Folder
Go to: `C:\Users\tiffa\BookHub\BookHub.Presentation\wwwroot\images\`

### Step 2: Naming Convention
Use one of these naming formats for your cover images:

**Option 1: By Book ID**
- `book_1.jpg` (for book with ID 1)
- `book_2.png` (for book with ID 2)
- `book_3.jpeg` (for book with ID 3)

**Option 2: By Book Title** (clean filename)
- `legends_lattes.jpg` (for "Legends & Lattes")
- `the_hobbit.png` (for "The Hobbit")
- `harry_potter.webp` (for "Harry Potter")

### Step 3: Supported File Formats
- `.jpg` / `.jpeg`
- `.png` 
- `.webp`

### Step 4: Recommended Image Size
- **Width**: 300-600px
- **Height**: 400-800px
- **Aspect Ratio**: Book-like proportions (roughly 2:3)

### Step 5: How the System Works
1. **Check by Book ID first**: Looks for `book_{BookId}.{extension}`
2. **Fallback to title**: Looks for clean title filename
3. **Show gradient**: If no cover found, shows beautiful gradient with book info

### Example File Names:
```
book_1.jpg          ← Book ID 1
book_2.png          ← Book ID 2
legends_lattes.jpg  ← Clean title version
the_hobbit.png      ← Clean title version
```

### Tips:
- ✅ Use consistent naming (prefer book ID method)
- ✅ Keep file sizes reasonable (under 500KB each)
- ✅ Use standard image formats
- ✅ Books without covers will show beautiful gradients automatically

### Finding Your Book IDs:
Check the browser's developer tools (F12) or look at the book card's data attributes to find the exact Book ID for each book.