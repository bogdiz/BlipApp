const actualBtn = document.getElementsByClassName('custom-file-input');

const fileChosen1 = document.getElementById('file-chosen1');
const fileChosen2 = document.getElementById('file-chosen2');


actualBtn[0].addEventListener('change', function () {
    fileChosen1.textContent = this.files[0].name
})

actualBtn[1].addEventListener('change', function () {
    fileChosen2.textContent = this.files[0].name
})

function handleCardClick(event, url) {
    window.location.href = url;
    event.stopPropagation();
}

function likePost(event, Id) {
    $.ajax({
        type: "POST",
        url: "/Posts/HandleLikes/" + Id,
        success: function (result) {
            $("#likeCount_" + Id).text(result.likes + " likes");
        },
        error: function (error) {
            console.error("Error liking post: " + error);
        }
    });
    event.stopPropagation();
}
function updateProfileVisibility() {
    var switchInput = document.getElementById('ProfileVisibility');
    document.getElementById('switchLabel').innerText = switchInput.checked ? 'Private' : 'Public';

    
    var profileVisibilityInput = document.getElementById('ProfileVisibility');
    profileVisibilityInput.value = switchInput.checked;

}

function searchString(id) {
    var input = document.getElementById(id);
    var string = input.value;
    var searchUrl = '/ApplicationUsers/Search/' + encodeURIComponent(string);
    window.location.href = searchUrl;
}
