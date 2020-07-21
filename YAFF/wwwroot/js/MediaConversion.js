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

function postFileToWebRoot() {
    //maybe make better use of forms lol
    var fdata = new FormData()
    fdata.append("file", $("#submitFile")[0].files[0])
    $.ajax({
        url: "api/File/",
        type: "POST",
        data: fdata,
        processData: false,
        contentType: false,
        success: function (data) {
            //TODO somehow await processing of file then do a nice little GET and serve the file. easy
            console.log('upload complete!')
        },
        error: function (data) {
            alert(`Error creating database record! File not uploaded. ${data.status}: ${data.statusText}`)
            console.log(data.responseText)
            //TODO delete the output info
        }
    })
}

function postFileToDb() {
    //https://stackoverflow.com/questions/21060247/send-formdata-and-string-data-together-through-jquery-ajax
    //https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-3.1
    var fdata = new FormData()
    fdata.append("file", $("#submitFile")[0].files[0])
    $.ajax({
        url: "api/File/",
        type: "POST",
        data: fdata,
        processData: false,
        contentType: false,
        success: function (data) {
            //TODO somehow await processing of file then do a nice little GET and serve the file. easy
            console.log('upload complete!')
        },
        error: function (data) {
            alert(`Error creating database record! File not uploaded. ${data.status}: ${data.statusText}`)
            console.log(data.responseText)
            //TODO delete the output info
        }
    })
}


$("#submitButton").click(function () {
    console.log(`submitting record for file: ${$("#submitOutName").val() + $("#submitExt").val()}`)
    //TODO...this part isnt working
    $.ajax({
        url: "api/Media/",
        type: "POST",
        data: JSON.stringify({
            "outputName": $("#submitOutName").val(),
            "outputExtension": $("#submitExt").val(),
        }),
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            console.log(data)
            console.log(`Record submitted for ID ${data.id}! Now posting file...`)
            //TODO--now that table record has been created, do another POST for the file itself--somehow get the ID incorporated there
            
        },
        error: function (data) {
            alert(`Error creating database record! Info not uploaded. ${data.status}: ${data.statusText}`)
            console.log(data.responseText)
        }
    })
})