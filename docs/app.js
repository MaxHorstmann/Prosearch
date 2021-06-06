const Controller = {
  search: (ev) => {
    ev.preventDefault();
    const form = document.getElementById("form");
    const data = Object.fromEntries(new FormData(form));
    const url = "https://maxhorstmann-prosearch.azurewebsites.net/api/prosearch?code=fakeZ2RzMPXhtl89lLnHQ4BMOjjffCCea19Y375XaSXCNr5WyWGITQ==";
    const response = fetch(url+`&q=${data.query}`, { mode: 'cors'}).then((response) => {
      response.json().then((results) => {
        Controller.updateTable(results);
      });
    });
  },

  updateTable: (results) => {
    const table = document.getElementById("table-body");
    table.innerHTML = '';
    // var totalHits = results.hits.total.value;
    // let hitsRow = document.createElement('tr');
    // hitsRow.innerHTML = `<tr><p style='font-weight:bold'>${totalHits} results found.</p></tr>`
    // table.appendChild(hitsRow);

    for (let result of results) {
      let row = document.createElement('tr');
      row.innerHTML = `<tr><td><img src="${result.providerImageUrl}" alt="${result.provider}"></td><td><p><a href="${result.url}">${result.title}</a></p></td></tr>`;
      table.appendChild(row);
    }
  },
};

const form = document.getElementById("form");
form.addEventListener("submit", Controller.search);