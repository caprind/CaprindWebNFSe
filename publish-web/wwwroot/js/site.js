// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/**
 * Exibe um toast com a mensagem especificada
 * @param {string} message - Mensagem a ser exibida
 * @param {string} type - Tipo do toast: 'success', 'error', 'info', 'warning'
 */
function showToast(message, type = 'info') {
    if (!message) return;
    
    // Configurações por tipo
    const toastConfig = {
        success: {
            bgClass: 'bg-success',
            icon: '<span class="me-2">✓</span>',
            header: 'Sucesso'
        },
        error: {
            bgClass: 'bg-danger',
            icon: '<span class="me-2">✕</span>',
            header: 'Erro'
        },
        info: {
            bgClass: 'bg-info',
            icon: '<span class="me-2">ℹ</span>',
            header: 'Informação'
        },
        warning: {
            bgClass: 'bg-warning',
            icon: '<span class="me-2">⚠</span>',
            header: 'Aviso'
        }
    };

    const config = toastConfig[type] || toastConfig.info;
    const toastId = 'toast-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9);

    // Criar o HTML do toast
    const toastHTML = `
        <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true" data-bs-autohide="true" data-bs-delay="5000">
            <div class="toast-header ${config.bgClass} text-white">
                ${config.icon}
                <strong class="me-auto">${config.header}</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
            </div>
        </div>
    `;

    // Adicionar ao container (centro da tela)
    const container = document.getElementById('toastContainer');
    if (!container) {
        console.error('Toast container não encontrado');
        return;
    }
    
    // Limpa toasts anteriores para mostrar apenas um de cada vez no centro
    container.innerHTML = '';
    
    container.insertAdjacentHTML('beforeend', toastHTML);

    // Inicializar e mostrar o toast
    const toastElement = document.getElementById(toastId);
    const toastBody = toastElement.querySelector('.toast-body');
    
    // Permite HTML (mensagens vêm do servidor, então são confiáveis)
    toastBody.innerHTML = message;
    
    const toast = new bootstrap.Toast(toastElement);
    toast.show();

    // Remover o elemento do DOM após ser escondido
    toastElement.addEventListener('hidden.bs.toast', function () {
        toastElement.remove();
    });
}