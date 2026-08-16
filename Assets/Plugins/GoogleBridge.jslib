mergeInto(LibraryManager.library, {
    
    ShowGoogleLoginButton: function () {
        var container = document.getElementById('google-login-container');
        if (container) {
            container.style.display = 'block';
        }
    },

    HideGoogleLoginButton: function () {
        var container = document.getElementById('google-login-container');
        if (container) {
            container.style.display = 'none';
        }
    }
});