(function() {
    'use strict'; 
    const CONFIG = {
        ANIMATION_DURATION: 300,  
        DEBOUNCE_DELAY: 250,      
        TOAST_DURATION: 5000,     
        API_TIMEOUT: 10000        
    };
    function debounce(func, wait, immediate) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                timeout = null;
                if (!immediate) func.apply(this, args);
            };
            const callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(this, args);
        };
    }
    function throttle(func, limit) {
        let inThrottle;
        return function(...args) {
            if (!inThrottle) {
                func.apply(this, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    }
    async function enhancedFetch(url, options = {}) {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), CONFIG.API_TIMEOUT);
        try {
            const response = await fetch(url, {
                ...options,
                signal: controller.signal
            });
            clearTimeout(timeoutId);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response;
        } catch (error) {
            clearTimeout(timeoutId);
            throw error;
        }
    }
    function showToast(type = 'info', title, message, duration = CONFIG.TOAST_DURATION) {
        const toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            console.warn('Toast container not found');
            return;
        }
        const toastId = `toast-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        const iconMap = {
            success: 'bi-check-circle-fill',
            error: 'bi-exclamation-triangle-fill',
            warning: 'bi-exclamation-triangle-fill',
            info: 'bi-info-circle-fill'
        };
        const toastHtml = `
            <div id="${toastId}" 
                 class="toast align-items-center border-0 bg-${type}" 
                 role="alert" 
                 aria-live="assertive" 
                 aria-atomic="true"
                 data-bs-autohide="true"
                 data-bs-delay="${duration}">
                <div class="d-flex">
                    <div class="toast-body text-white d-flex align-items-center">
                        <i class="bi ${iconMap[type]} me-2" aria-hidden="true"></i>
                        <div>
                            <strong>${title}</strong>
                            ${message ? `<div class="small opacity-75">${message}</div>` : ''}
                        </div>
                    </div>
                    <button type="button" 
                            class="btn-close btn-close-white me-2 m-auto" 
                            data-bs-dismiss="toast" 
                            aria-label="Close notification">
                    </button>
                </div>
            </div>
        `;
        toastContainer.insertAdjacentHTML('beforeend', toastHtml);
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement);
        toastElement.classList.add('fade-in-up');
        toast.show();
        toastElement.addEventListener('hidden.bs.toast', function() {
            toastElement.remove();
        });
        setTimeout(() => {
            if (document.getElementById(toastId)) {
                document.getElementById(toastId).remove();
            }
        }, duration + 1000);
    }
    function validateForm(form) {
        if (!form) return false;
        let isValid = true;
        const requiredFields = form.querySelectorAll('[required]');
        form.querySelectorAll('.is-invalid').forEach(field => {
            field.classList.remove('is-invalid');
        });
        form.querySelectorAll('.invalid-feedback').forEach(feedback => {
            feedback.remove();
        });
        requiredFields.forEach(field => {
            if (!field.value.trim()) {
                isValid = false;
                field.classList.add('is-invalid');
                const feedback = document.createElement('div');
                feedback.className = 'invalid-feedback';
                feedback.textContent = `${field.labels[0]?.textContent || field.name} is required.`;
                field.parentNode.appendChild(feedback);
            } else {
                field.classList.add('is-valid');
            }
        });
        const emailFields = form.querySelectorAll('input[type="email"]');
        emailFields.forEach(field => {
            if (field.value && !isValidEmail(field.value)) {
                isValid = false;
                field.classList.add('is-invalid');
                const feedback = document.createElement('div');
                feedback.className = 'invalid-feedback';
                feedback.textContent = 'Please enter a valid email address.';
                field.parentNode.appendChild(feedback);
            }
        });
        return isValid;
    }
    function isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }
    function animateIntoView(element, animationClass = 'fade-in-up') {
        if (!element) return;
        element.classList.add(animationClass, 'visible');
        setTimeout(() => {
            element.classList.remove(animationClass);
        }, CONFIG.ANIMATION_DURATION * 2);
    }
    function toggleLoading(element, show = true) {
        if (!element) return;
        if (show) {
            element.disabled = true;
            element.setAttribute('data-original-text', element.textContent);
            element.innerHTML = `
                <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                Loading...
            `;
        } else {
            element.disabled = false;
            element.textContent = element.getAttribute('data-original-text') || 'Submit';
            element.removeAttribute('data-original-text');
        }
    }
    function initializeApp() {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
        const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
        popoverTriggerList.map(function (popoverTriggerEl) {
            return new bootstrap.Popover(popoverTriggerEl);
        });
        const forms = document.querySelectorAll('form');
        forms.forEach(form => {
            form.setAttribute('novalidate', 'novalidate');
            const inputs = form.querySelectorAll('input, textarea, select');
            inputs.forEach(input => {
                input.addEventListener('blur', function() {
                    if (this.hasAttribute('required') && !this.value.trim()) {
                        this.classList.add('is-invalid');
                    } else {
                        this.classList.remove('is-invalid');
                        this.classList.add('is-valid');
                    }
                });
            });
        });
        const searchInputs = document.querySelectorAll('input[type="search"], .search-input');
        searchInputs.forEach(input => {
            const searchFunction = debounce(function(e) {
                const query = e.target.value.trim();
                if (query.length >= 2) {
                    performSearch(query);
                }
            }, CONFIG.DEBOUNCE_DELAY);
            input.addEventListener('input', searchFunction);
        });
        const anchorLinks = document.querySelectorAll('a[href^="#"]');
        anchorLinks.forEach(link => {
            link.addEventListener('click', function(e) {
                const href = this.getAttribute('href');
                if (href === '#') return;
                const target = document.querySelector(href);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape') {
                const openModal = document.querySelector('.modal.show');
                if (openModal) {
                    bootstrap.Modal.getInstance(openModal)?.hide();
                }
                const openDropdown = document.querySelector('.dropdown-menu.show');
                if (openDropdown) {
                    bootstrap.Dropdown.getInstance(openDropdown.previousElementSibling)?.hide();
                }
            }
        });
        if ('performance' in window) {
            window.addEventListener('load', function() {
                setTimeout(function() {
                    const perfData = performance.getEntriesByType('navigation')[0];
                    if (perfData) {
                        console.log(`Page load time: ${perfData.loadEventEnd - perfData.loadEventStart}ms`);
                    }
                }, 0);
            });
        }
        if ('IntersectionObserver' in window) {
            const observer = new IntersectionObserver(function(entries) {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        animateIntoView(entry.target);
                        observer.unobserve(entry.target);
                    }
                });
            }, {
                threshold: 0.1,
                rootMargin: '0px 0px -50px 0px'
            });
            const animateElements = document.querySelectorAll('.animate-on-scroll, .card, .hero-section');
            animateElements.forEach(el => {
                observer.observe(el);
            });
        }
    }
    function performSearch(query) {
        console.log(`Searching for: ${query}`);
    }
    window.addEventListener('error', function(event) {
        console.error('Global error:', event.error);
        showToast('error', 'Error', 'An unexpected error occurred. Please try again.');
    });
    window.addEventListener('unhandledrejection', function(event) {
        console.error('Unhandled promise rejection:', event.reason);
        showToast('error', 'Error', 'A network error occurred. Please check your connection.');
    });
    window.BookHub = {
        showToast,
        validateForm,
        toggleLoading,
        enhancedFetch,
        debounce,
        throttle,
        animateIntoView
    };
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeApp);
    } else {
        initializeApp();
    }
})();
