document.addEventListener('DOMContentLoaded', () => {
   
    const pageTemplatePromise = getPageTemplate() // 1
        .then((template) => { // 3
            const pageBody = document.getElementById('pageBody');
            if (pageBody == null) {
                throw "DOMContentLoaded: pageBody element not found";
            }
            pageBody.innerHTML = template;
        });

    const pageContainerPromise = Promise.all([
        getPageContent(), // 2
        void pageTemplatePromise, // 4
    ])
        .then(([content]) => { // 5
            const container = getPageContainer();
            container.innerHTML = content;
        });


    window.addEventListener("hashchange", async (e) => { // 3
        const container = getPageContainer();
        container.innerHTML = await getPageContent();
    });
});


async function getPageTemplate() {
    return fetch('/tpl/forum-index.html').then(r => r.text())
}
async function getPageContent() {
    const path = window.location.hash.substring(1).split('/');
    /* path[0] - Controller (Section/Topic/Theme)
       path[1] - id
    */
    console.log(path);
    switch (path[0].toLowerCase()) {
        case 'section': return loadTopics(path[1]);
        case 'topic': return loadThemes(path[1]);
        default: return loadSections()
    }
}

function getPageContainer() {
    const container = document.getElementById('sections');
    if (!container) throw "loadSections(): Container 'sections' not found";
    return container;
}


async function loadSections() {
    return getTemplate('/tpl/forum-section-view.html', '/api/section')
}

async function loadTopics(sectionId) {
    let url = new URL('/api/topic', window.location);
    url.searchParams.set('sectionId', sectionId)
    return getTemplate('/tpl/forum-topic-view.html', url)

}

async function loadThemes(topicId) {
    return `${topicId} will come soon`;

    //let url = new URL('/api/theme', window.location);
    //url.searchParams.set('topicId', topicId)
    //return getTemplate('/tpl/forum-theme-view.html', url)

}


async function loadTemplateData(dataUrl) {
    return fetch(dataUrl, {
        method: 'GET'
    })
        .then(r => r.json())
}

async function loadTemplateHTML(templateUrl) {
    return fetch(templateUrl).then(r => r.text())
}

async function getTemplate(templateUrl, dataUrl) {
    let [j, t] = await Promise.all([
        loadTemplateData(dataUrl),
        loadTemplateHTML(templateUrl)
    ]);

    // j - данные, t - шаблон для заполнения данными
    let content = "";
    for (let section of j) {
        let item = t;  // copy of template
        for (let prop in section) {
            // console.log(prop + " " + typeof section[prop]);
            if ('object' === typeof section[prop]) {
                for (let subprop in section[prop]) {
                    item = item.replaceAll(
                        `{{${prop}.${subprop}}}`,
                        section[prop][subprop]
                    );
                }
            }
            else {
                item = item.replaceAll(`{{${prop}}}`, section[prop]);
            }
        }
        content += item;
    }
    return content;
}
/*
Реализовать заполнение шаблона секции дополнительными данными:
аватарка автора, его дата регистрации (на форуме с... / на форуме .. дней)
* статистику: кол-во топиков
** общее кол-во просмотров
*/