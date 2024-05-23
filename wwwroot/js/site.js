document.addEventListener('DOMContentLoaded', () => {
    const authButton = document.getElementById("auth-button");
    if (authButton) authButton.addEventListener('click', authButtonClick);

    const saveProfileButton = document.getElementById("profile-save-button");
    if (saveProfileButton) saveProfileButton.addEventListener('click', saveProfileButtonClick);

    const signoutButton = document.getElementById("auth-signout-button");
    if (signoutButton) signoutButton.addEventListener('click',
        function () {
          
            const confirmed = confirm('Ви впевнені, що хочете вийти?');

            if (confirmed) {
                signOutButtonClick();
            }
        });

    document.body.addEventListener('click', function (event) {
        if (event.target && event.target.matches('.move.link-style')) {
            const adsId = event.target.getAttribute('data-ads-id');
            const confirmed = confirm('Ви впевнені, що хочете видалити відповідне оголошення?');
            if (confirmed) {
                deleteAdsButtonClick(adsId);
            } 
        }
    });

});

function deleteAdsButtonClick(adsId) {

    fetch(`/Ads/DeleteAds/?adsId=${encodeURIComponent(adsId)}`, {
        method: 'DELETE'
    })
        .then(r => r.json())
        .then(j => {
            console.log(j);
            window.location.reload();
        })
        .catch(error => {
            console.error('Error:', error);
        });

}

function signOutButtonClick() {

    fetch('/api/auth', {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => {
            if (response.ok) {
                
                window.location.assign('/');       
            } else {
                return response.json();
            }
        })
        .then(data => {
            console.log(data);
            showSignoutMessage();
        })
        .catch(error => {
            console.error('Ошибка:', error);
        });
}


function saveProfileButtonClick() {
    const nameInput = document.querySelector('input[name="profile-name"]');
    if (!nameInput) throw 'Element input[name="profile-name"] not found';
    const emailInput = document.querySelector('input[name="profile-email"]');
    if (!emailInput) throw 'Element input[name="profile-email"] not found';
    fetch(
        `/Home/UpdateProfile?newName=${nameInput.value}&newEmail=${emailInput.value}`,
        {
            method: 'POST'
        })
        .then(r => r.json())
        .then(j => {
            console.log(j);
            window.location.reload();
            showSaveMessage("Зміни збережено");
        });
}

function authButtonClick() {
    const loginInput = document.getElementById("auth-login");
    if (!loginInput) throw "Element #auth-login not found";
    const login = loginInput.value.trim();
    if (login.length == 0) {
        showAuthMessage("Логін не може бути порожнім");
        return;
    }
    const passwordInput = document.getElementById("auth-password");
    if (!passwordInput) throw "Element #auth-password not found";
    const password = passwordInput.value.trim();
    if (password.length == 0) {
        showAuthMessage("Пароль не може бути порожнім");
        return;
    }
    fetch(`/api/auth?login=${login}&password=${password}`)
        .then(r => {
            if (r.status == 200) {  // OK
               
                window.location.reload();    
            }
            else {  // 401
                showAuthMessage("Вхід відхилено");
            }
           
        });
}
function showAuthMessage(message) {
    const authMessage = document.getElementById("auth-message");
    if (!authMessage) throw "Element #auth-message not found";
    authMessage.innerText = message;
    authMessage.classList.remove("visually-hidden");
}
function showSaveMessage(message) {
    const saveMessage = document.getElementById("save-message");
    if (!saveMessage) throw "Element #auth-message not found";
    saveMessage.innerText = message;
    saveMessage.classList.remove("visually-hidden");
}