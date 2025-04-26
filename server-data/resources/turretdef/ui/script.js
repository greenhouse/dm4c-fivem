window.addEventListener('message', (event) => {
    const data = event.data;
    
    if (data.type === 'init' || data.type === 'update') {
        document.getElementById('kill-count').textContent = data.killCount;
        document.getElementById('level').textContent = data.level;
    }
    
    if (data.type === 'levelChange') {
        const notification = document.getElementById('notification');
        notification.textContent = `Level ${data.level} Increase`;
        notification.classList.remove('hidden');
        setTimeout(() => {
            notification.classList.add('hidden');
        }, 5000);
    }
});