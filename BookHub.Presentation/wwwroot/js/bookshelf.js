// Bookshelf JavaScript functionality
document.addEventListener('DOMContentLoaded', function() {
    // View switching
    const gridViewBtn = document.getElementById('gridView');
    const shelfViewBtn = document.getElementById('shelfView');
    const gridViewContainer = document.getElementById('gridViewContainer');
    const shelfViewContainer = document.getElementById('shelfViewContainer');
    
    // Search and filter elements
    const searchInput = document.getElementById('bookSearch');
    const authorFilter = document.getElementById('authorFilter');
    const genreFilter = document.getElementById('genreFilter');
    const sortSelect = document.getElementById('sortBy');
    const clearFiltersBtn = document.getElementById('clearFilters');
    
    // Store all books for filtering
    let allBooks = [];
    let currentView = 'grid';
    
    // Initialize
    init();
    
    function init() {
        // Collect all books data
        collectBooksData();
        
        // Populate filters
        populateAuthorFilter();
        populateGenreFilter();
        
        // Set up event listeners
        setupEventListeners();
        
        // Apply initial view
        switchView('grid');
    }
    
    function collectBooksData() {
        const bookItems = document.querySelectorAll('.book-item, .book-list-item');
        allBooks = Array.from(bookItems).map(item => ({
            element: item,
            title: item.dataset.title || '',
            author: item.dataset.author || '',
            genre: item.dataset.genre || ''
        }));
    }
    
    function populateAuthorFilter() {
        const authors = [...new Set(allBooks.map(book => book.author))].sort();
        authorFilter.innerHTML = '<option value="">All Authors</option>';
        authors.forEach(author => {
            const option = document.createElement('option');
            option.value = author;
            option.textContent = capitalizeWords(author);
            authorFilter.appendChild(option);
        });
    }
    
    function populateGenreFilter() {
        const genres = [...new Set(allBooks.map(book => book.genre).filter(genre => genre))].sort();
        genreFilter.innerHTML = '<option value="">All Genres</option>';
        genres.forEach(genre => {
            const option = document.createElement('option');
            option.value = genre;
            option.textContent = capitalizeWords(genre);
            genreFilter.appendChild(option);
        });
    }
    
    function setupEventListeners() {
        // View switching
        gridViewBtn.addEventListener('click', () => switchView('grid'));
        shelfViewBtn.addEventListener('click', () => switchView('shelf'));
        
        // Search and filters
        searchInput.addEventListener('input', filterBooks);
        authorFilter.addEventListener('change', filterBooks);
        genreFilter.addEventListener('change', filterBooks);
        sortSelect.addEventListener('change', sortAndFilterBooks);
        clearFiltersBtn.addEventListener('click', clearFilters);
        
        // Book interactions
        setupBookInteractions();
    }
    
    function switchView(view) {
        currentView = view;
        
        if (view === 'grid') {
            gridViewContainer.style.display = 'block';
            shelfViewContainer.style.display = 'none';
            gridViewBtn.classList.add('active');
            shelfViewBtn.classList.remove('active');
        } else {
            gridViewContainer.style.display = 'none';
            shelfViewContainer.style.display = 'block';
            gridViewBtn.classList.remove('active');
            shelfViewBtn.classList.add('active');
        }
        
        // Reapply filters for new view
        filterBooks();
    }
    
    function filterBooks() {
        const searchTerm = searchInput.value.toLowerCase();
        const selectedAuthor = authorFilter.value;
        const selectedGenre = genreFilter.value;
        
        const filteredBooks = allBooks.filter(book => {
            const matchesSearch = book.title.includes(searchTerm) || book.author.includes(searchTerm) || book.genre.includes(searchTerm);
            const matchesAuthor = !selectedAuthor || book.author === selectedAuthor;
            const matchesGenre = !selectedGenre || book.genre === selectedGenre;
            
            return matchesSearch && matchesAuthor && matchesGenre;
        });
        
        // Hide all books first
        allBooks.forEach(book => {
            book.element.style.display = 'none';
        });
        
        // Show filtered books
        filteredBooks.forEach(book => {
            book.element.style.display = '';
        });
        
        // Update results count
        updateResultsCount(filteredBooks.length);
    }
    
    function sortAndFilterBooks() {
        const sortBy = sortSelect.value;
        
        // Sort the books
        allBooks.sort((a, b) => {
            switch (sortBy) {
                case 'title':
                    return a.title.localeCompare(b.title);
                case 'author':
                    return a.author.localeCompare(b.author);
                case 'genre':
                    return a.genre.localeCompare(b.genre);
                default:
                    return 0;
            }
        });
        
        // Reorder DOM elements
        const container = currentView === 'grid' 
            ? document.getElementById('booksContainer')
            : document.querySelector('.books-list');
            
        if (container) {
            allBooks.forEach(book => {
                container.appendChild(book.element);
            });
        }
        
        // Apply filters after sorting
        filterBooks();
    }
    
    function clearFilters() {
        searchInput.value = '';
        authorFilter.value = '';
        genreFilter.value = '';
        sortSelect.value = 'title';
        
        // Show all books
        allBooks.forEach(book => {
            book.element.style.display = '';
        });
        
        updateResultsCount(allBooks.length);
    }
    
    function updateResultsCount(count) {
        const countElement = document.querySelector('.book-count');
        if (countElement) {
            countElement.textContent = `(${count} books)`;
        }
    }
    
    function setupBookInteractions() {
        // Add click handlers for book details
        document.addEventListener('click', function(e) {
            const bookCard = e.target.closest('.book-card');
            const bookListItem = e.target.closest('.book-list-item');
            
            if (bookCard || bookListItem) {
                const element = bookCard || bookListItem;
                const title = element.querySelector('[data-title]')?.dataset.title || 'Unknown';
                const author = element.querySelector('[data-author]')?.dataset.author || 'Unknown';
                const genre = element.querySelector('[data-genre]')?.dataset.genre || 'Unknown';
                
                showBookDetails(capitalizeWords(title), capitalizeWords(author), capitalizeWords(genre));
            }
        });
    }
    
    function showBookDetails(title, author, genre) {
        // Simple alert for now - could be enhanced with a modal
        alert(`${title}\nBy: ${author}\nGenre: ${genre || 'Unknown'}`);
    }
    
    function capitalizeWords(str) {
        return str.replace(/\w\S*/g, (txt) => 
            txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase()
        );
    }
    
    // Add smooth animations
    function addAnimations() {
        const books = document.querySelectorAll('.book-card, .book-list-item');
        
        books.forEach((book, index) => {
            book.style.animationDelay = `${index * 0.02}s`;
            book.classList.add('book-fade-in');
        });
    }
    
    // Initialize animations after a short delay
    setTimeout(addAnimations, 100);
});

// Add CSS animation class
const style = document.createElement('style');
style.textContent = `
    @keyframes fadeInUp {
        from {
            opacity: 0;
            transform: translateY(20px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }
    
    .book-fade-in {
        animation: fadeInUp 0.6s ease forwards;
    }
    
    .book-card, .book-list-item {
        opacity: 0;
    }
    
    .book-fade-in {
        opacity: 1;
    }
`;
document.head.appendChild(style);