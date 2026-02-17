// Sistema de Troca de Temas

(function() {
    'use strict';

    const THEME_STORAGE_KEY = 'nfse-theme';
    const DEFAULT_THEME = 'blue';

    // Definir cores de cada tema para preview
    const themes = {
        blue: { name: 'Azul', color: '#3b82f6' },
        red: { name: 'Vermelho', color: '#ef4444' },
        green: { name: 'Verde', color: '#10b981' },
        yellow: { name: 'Amarelo', color: '#f59e0b' },
        dark: { name: 'Escuro', color: '#6366f1' }
    };

    // Obter tema salvo ou usar o padrão
    function getSavedTheme() {
        try {
            const saved = localStorage.getItem(THEME_STORAGE_KEY);
            return saved && themes[saved] ? saved : DEFAULT_THEME;
        } catch (e) {
            return DEFAULT_THEME;
        }
    }

    // Aplicar tema ao documento
    function applyTheme(themeName) {
        const html = document.documentElement;
        const body = document.body;

        // Remove todos os temas
        Object.keys(themes).forEach(theme => {
            html.removeAttribute('data-theme');
            body.removeAttribute('data-theme');
        });

        // Aplica o tema selecionado
        if (themeName && themeName !== 'blue') {
            html.setAttribute('data-theme', themeName);
            body.setAttribute('data-theme', themeName);
        }

        // Salva no localStorage
        try {
            localStorage.setItem(THEME_STORAGE_KEY, themeName);
        } catch (e) {
            console.warn('Não foi possível salvar o tema:', e);
        }

        // Atualiza o seletor visual
        updateThemeSelector(themeName);
    }

    // Criar seletor de temas
    function createThemeSelector() {
        const selector = document.createElement('div');
        selector.className = 'theme-selector';

        const button = document.createElement('button');
        button.className = 'btn btn-sm btn-outline-light theme-toggle';
        button.setAttribute('type', 'button');
        button.setAttribute('aria-label', 'Trocar tema');
        button.innerHTML = '🎨 Tema';
        button.onclick = function(e) {
            e.stopPropagation();
            toggleThemeOptions();
        };

        const options = document.createElement('div');
        options.className = 'theme-options';
        options.id = 'themeOptions';

        Object.keys(themes).forEach(themeKey => {
            const option = document.createElement('div');
            option.className = 'theme-option';
            option.setAttribute('data-theme', themeKey);
            
            const preview = document.createElement('div');
            preview.className = 'theme-color-preview';
            preview.style.backgroundColor = themes[themeKey].color;
            
            const label = document.createElement('span');
            label.className = 'theme-option-label';
            label.textContent = themes[themeKey].name;
            
            const check = document.createElement('span');
            check.className = 'theme-check';
            check.innerHTML = '✓';
            check.style.display = 'none';
            
            option.appendChild(preview);
            option.appendChild(label);
            option.appendChild(check);
            
            option.onclick = function() {
                applyTheme(themeKey);
                toggleThemeOptions();
            };
            
            options.appendChild(option);
        });

        selector.appendChild(button);
        selector.appendChild(options);

        return selector;
    }

    // Alternar visibilidade do menu de opções
    function toggleThemeOptions() {
        const options = document.getElementById('themeOptions');
        if (options) {
            options.classList.toggle('show');
        }
    }

    // Atualizar seletor visual
    function updateThemeSelector(activeTheme) {
        const options = document.querySelectorAll('.theme-option');
        options.forEach(option => {
            const theme = option.getAttribute('data-theme');
            const check = option.querySelector('.theme-check');
            
            option.classList.remove('active');
            if (check) {
                check.style.display = 'none';
            }

            if (theme === activeTheme) {
                option.classList.add('active');
                if (check) {
                    check.style.display = 'inline';
                }
            }
        });
    }

    // Fechar menu ao clicar fora
    document.addEventListener('click', function(e) {
        const selector = document.querySelector('.theme-selector');
        if (selector && !selector.contains(e.target)) {
            const options = document.getElementById('themeOptions');
            if (options) {
                options.classList.remove('show');
            }
        }
    });

    // Inicializar quando o DOM estiver pronto
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    function init() {
        // Aplicar tema salvo
        const savedTheme = getSavedTheme();
        applyTheme(savedTheme);

        // Adicionar seletor ao navbar (se não existir)
        setTimeout(function() {
            const navbar = document.querySelector('.navbar-nav:last-child');
            if (navbar && !document.querySelector('.theme-selector')) {
                const themeSelector = createThemeSelector();
                const li = document.createElement('li');
                li.className = 'nav-item';
                li.style.cssText = 'margin-right: 0.5rem; display: flex; align-items: center;';
                li.appendChild(themeSelector);
                
                // Insere antes do primeiro item (usuário ou login)
                const firstChild = navbar.firstElementChild;
                if (firstChild) {
                    navbar.insertBefore(li, firstChild);
                } else {
                    navbar.appendChild(li);
                }
            }
        }, 100);
    }

    // Expor função globalmente para uso externo (se necessário)
    window.applyTheme = applyTheme;
    window.getSavedTheme = getSavedTheme;

})();

