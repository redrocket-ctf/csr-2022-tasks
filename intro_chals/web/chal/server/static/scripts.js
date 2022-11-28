let opinion = document.getElementById("opinion");

fetch("/get_opinion", {
    method: "get"
}).then((response) => {
    if(response.status == 200) {
        return response.text();
    } else {
        return "err";
    }
}).then((text) => {
    opinion.innerHTML = text;
});

function patch_me_out_in_production( value ) {
    fetch("/set_opinion", {
        method: "post",
        body: new URLSearchParams({value}),
    });
}
