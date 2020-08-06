var subtitles = [
    "It's so easy to use",
    "You like it?",
    "Wow!",
    "Doesn't mine crypto in the background like the one I used to use",
    "DO NOT Google \"audio file converter\"",
]

//Acceptable file extensions
var extensions = [
    ".mp3",
    ".wav",
    ".flac",
    ".ogg",
    ".aiff"
]

$(document).ready(function () {
    //populate file extension dropdown
    var htmlExtensions = ""
    extensions.forEach(item => htmlExtensions = htmlExtensions.concat(`<a class="dropdown-item" href="#">${item}</a>`))
    $("#submitExtDropdown").html(htmlExtensions)
    //populate subtitle
    $("#subtitle").html(subtitles[Math.floor(Math.random() * subtitles.length)])
})

//when file is uploaded, allow use of dropdown, and prepopulate filename
//TODO maybe hide the rest of the UI
$("#submitFile").on("change", function () {
    if ($("#submitFile").val()) {
        $("#submitExt").removeAttr("disabled")
        $("#submitOutName").val($("#submitFile").val().replace(/\.[^/.]+$/, "").replace(/^.*[\\\/]/, ''))//TODO this is definitely not the best way of doing this
        updateTextAndVal($("#submitExt"), ".mp3") //set to .mp3 by default
    }
})

//when dropdown element is clicked, update the text for the button
$("#submitExtDropdown a").on("click", function () {
    updateTextAndVal($("#submitExt"), $(this).text())
});

$("#submitButton").click(function () {
    if (!$("#submitButton").hasClass("disabled")) {
        convertFile()
    }
})

//this will save SO much time
function updateTextAndVal(element, value) {
    element.text(value)
    element.val(value)
}

function convertFile() {
    var fdata = new FormData()
    fdata.append("data", $("#submitFile")[0].files[0])
    fdata.append("outName", $("#submitOutName").val() + $("#submitExt").val())
    fdata.append("inName", $("#submitFile")[0].files[0].name)
    showAlert("Your file is being converted.", "Converting...")//TODO maybe replace this with progress bar if this takes forever after deployment
    $("#submitButton").addClass("disabled")//TODO reinforce this on server if need be
    $.ajax({
        cache: false,
        url: 'api/Media/PostFile',
        type: "POST",
        data: fdata,
        processData: false,
        contentType: false,
        success: function (data) {
            //file upload and conversion worked! now ask for the converted file
            var location = 'api/Media/Download?fileGuid=' + data.fileGuid
                + '&filename=' + data.fileName
            console.log(location)
            if (data.fileName != null && data.fileGuid != null) {
                window.location.href = location
            }
            showAlert("Your file should be available shortly.","Conversion successful!","alert-success")//TODO add direct link if necessary
        },
        error: function (data) {
            console.log(data.responseText)
            showAlert(`Make sure you've uploaded a valid file type.<hr>Valid file types are: ${extensions.join(", ")}`, "Upload error!", "alert-danger")
        },
        always: function (data) {
            $("#submitButton").removeClass("disabled")
        }
    })
}

function showAlert(message, header = "Alert!", type = "alert-primary") {
    $('#alerts').html(
        `<div class="alert ${type} alert-dismissible fade show" role="alert">
            <strong>${header}</strong> ${message}
            <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>`
    )
}