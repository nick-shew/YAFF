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
    }
})

//when dropdown element is clicked, update the text for the button
$("#submitExtDropdown a").on("click", function () {
    $("#submitExt").text($(this).text())
    $("#submitExt").val($(this).text())
});


$("#submitButton").click(function () {
    console.log(`submitting record for file ${$("#submitOutName").val() + $("#submitExt").val()}`)
    //TODO...this part isnt working
    $.ajax({
        url: "/Home/PostOutputData",
        type: "POST",
        data: {
            "name": $("#submitOutName").val(),
            "extension": $("#submitExt").val(),
            //"inputFile": $("#submitFile")[0].files[0]
        },
        success: function (data) {
            //TODO--now that table record has been created, do another POST for the file itself--somehow get the ID incorporated there
        },
        error: function (data) {
            alert("Error creating database record! File not uploaded.")
        }
    })
})