//Acceptable file extensions
var extensions = [
    ".mp3",
    ".wav",
    ".flac",
    ".ogg",
    ".aiff"
]
var htmlExtensions = ""
extensions.forEach(item => htmlExtensions = htmlExtensions.concat(`<a class="dropdown-item" href="#">${item}</a>`))
$("#submitExtDropdown").html(htmlExtensions)

//when file is uploaded, allow use of dropdown, and prepopulate filename
//TODO maybe hide the rest of the UI
$("#submitFile").on("change", function () {
    if ($("#submitFile").val()) {
        $("#submitExt").removeAttr("disabled")
        $("#submitOutName").val($("#submitFile").val().replace(/\.[^/.]+$/, "").replace(/^.*[\\\/]/, ''))//TODO this is definitely not the best way of doing this
        updateTextAndVal($("#submitExt"), ".mp3") //DEBUG setting to .mp3 by default
    }
})

//when dropdown element is clicked, update the text for the button
$("#submitExtDropdown a").on("click", function () {
    //$("#submitExt").text($(this).text())
    //$("#submitExt").val($(this).text())
    updateTextAndVal($("#submitExt"),$(this).text())
});

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
    $.ajax({
        cache: false,
        url: 'api/Media/PostFile',
        type: "POST",
        data: fdata,
        processData: false,
        contentType: false,
        success: function (data) {
            console.log('Retrieving processed file...')
            //var response = JSON.parse(data)
            var response = data
            console.log(response)
            console.log(response.fileGuid)
            var location = 'api/Media/Download?fileGuid=' + response.fileGuid
                + '&filename=' + response.fileName
            console.log(location)
            if (response.fileName != null && response.fileGuid != null) {
                window.location = location
            }
        },
        error: function (data) {
            console.log(data.responseText)
        }
    })
}

$("#submitButton").click(function () {
    console.log(`submitting record for file: ${$("#submitOutName").val() + $("#submitExt").val()}`)
    convertFile()
})