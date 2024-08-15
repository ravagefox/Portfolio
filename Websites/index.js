class ChartManager {
    constructor() {
        this.chartInstances = {};
    }

    createChart(ctx, type, data, options) {
        return new Chart(ctx, {
            type: type,
            data: data,
            options: options
        });
    }

    addChart(chartId, type, data, options) {
        const ctx = document.getElementById(chartId).getContext('2d');
        this.chartInstances[chartId] = this.createChart(ctx, type, data, options);
    }
}

function createCard(title, chartData) {
    // Create card elements
    const card = document.createElement('div');
    card.classList.add('card');

    // Create card content
    const cardContent = document.createElement('div');
    cardContent.classList.add('card-content');

    const cardTitle = document.createElement('h5');
    cardTitle.textContent = title;
    cardContent.appendChild(cardTitle);

    const cardDescription = document.createElement('p');
    cardDescription.textContent = `Detailed information about ${title.toLowerCase()}.`;
    cardContent.appendChild(cardDescription);

    // Create card action
    const cardAction = document.createElement('div');
    cardAction.classList.add('card-action');

    const viewDetailsLink = document.createElement('a');
    viewDetailsLink.href = '';
    viewDetailsLink.classList.add('btn-small', 'waves-effect');
    viewDetailsLink.textContent = 'View Details';
    viewDetailsLink.onclick = function (e) {
        return toggleContent(e);
    };
    cardAction.appendChild(viewDetailsLink);

    // Create hidden content with chart canvas
    const hiddenContent = document.createElement('div');
    hiddenContent.classList.add('hidden-content');

    const canvas = document.createElement('canvas');
    canvas.classList.add('chart-canvas');
    canvas.id = `${title.replace(/\s+/g, '-').toLowerCase()}-chart`; // Generate a unique ID
    hiddenContent.appendChild(canvas);

    cardAction.appendChild(hiddenContent);

    // Append card content and action to the card
    card.appendChild(cardContent);
    card.appendChild(cardAction);

    // Append the card to the container
    document.getElementById('cards-container').appendChild(card);

    // Create chart for this card
    const chartManager = new ChartManager();
    chartManager.addChart(canvas.id, chartData.type, chartData.data, chartData.options);
}


function toggleContent(event) {
    event.preventDefault();

    const button = event.target;
    const hiddenContent = button.nextElementSibling;
    if (hiddenContent.style.display === "none") {
        hiddenContent.style.display = "block";
        button.textContent = "Hide Details";
    } else {
        hiddenContent.style.display = "none";
        button.textContent = "View Details";
    }

    return false;
}

document.addEventListener('DOMContentLoaded', function () {
    // Initialize Materialize components
    M.Sidenav.init(document.querySelectorAll('.sidenav'));
    M.Modal.init(document.querySelectorAll('.modal'));
    M.Tooltip.init(document.querySelectorAll('.tooltipped'));
    M.Dropdown.init(document.querySelectorAll('.dropdown-trigger'));

    // Example chart data for card
    const chartData = {
        type: 'bar',
        data: {
            labels: ['January', 'February', 'March', 'April', 'May', 'June'],
            datasets: [{
                label: 'Sample Data',
                data: [12, 19, 3, 5, 2, 3],
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    };

    const chartData2 = {
        type: 'line',
        data: {
            labels: ['January', 'February', 'March', 'April', 'May', 'June'],
            datasets: [{
                label: 'Sample Data',
                data: [12, 19, 3, 5, 2, 3],
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    };



    // Create dynamic cards
    createCard('Data Overview', chartData);
    createCard('Analytics', chartData2);
    createCard('Reports', chartData);

    document.getElementById('toggle-theme').addEventListener('click', toggleTheme);
    document.getElementById('toggle-sidebar').addEventListener('click', function () {
        const sidenav = document.querySelector('.sidenav');
        sidenav.classList.toggle('sidenav-collapsed');
        const content = document.querySelector('.content');
        content.classList.toggle('sidenav-collapsed');
    });
});

function toggleTheme() {
    const currentTheme = document.documentElement.getAttribute('data-theme');
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', newTheme);
    setCookie('theme', newTheme, 7);
}

function setCookie(name, value, days) {
    let expires = "";
    if (days) {
        const date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}