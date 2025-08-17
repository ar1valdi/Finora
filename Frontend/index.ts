function addUser() {
    const user = {
        firstName: (document.getElementById('firstName') as HTMLInputElement).value || '',
        secondName: (document.getElementById('secondName') as HTMLInputElement).value || '',
        lastName: (document.getElementById('lastName') as HTMLInputElement).value || '',
        dateOfBirth: (document.getElementById('dateOfBirth') as HTMLInputElement).value || '',
        email: (document.getElementById('email') as HTMLInputElement).value || '',
    }

    console.log(user);
}

function closeForm() {
    const form = document.getElementById('form');
    form?.classList.add('hidden');
}

function showForm() {
    const form = document.getElementById('form');
    form?.classList.remove('hidden');

    const users = document.getElementById('users');
    users?.classList.add('hidden');
}

async function showUsers() {
    const users = document.getElementById('users');
    users?.classList.remove('hidden');
    await fetch('http://localhost:5000/api/users')
        .then(response => response.json())
        .then(data => {
            data.forEach((user: any) => {
                const userElement = document.createElement('div');
                userElement.classList.add('user');
                userElement.innerHTML = `
                    <span class="user-name">${user.firstName}</span>
                    <span class="user-secondName">${user.secondName}</span>
                    <span class="user-lastName">${user.lastName}</span>
                    <span class="user-dateOfBirth">${user.dateOfBirth}</span>
                    <span class="user-email">${user.email}</span>
                `;
                users?.appendChild(userElement);
            });
        })
        .catch(error => {
            console.error('Error:', error);
        });
}