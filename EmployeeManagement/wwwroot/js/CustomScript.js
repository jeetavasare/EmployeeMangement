console.log("lob");
document.getElementsByTagName("div")[1].style.color = "Red";

function confirmDelete(uniqueId, isDeleteClicked) {
    var deleteSpan = 'deleteSpan_' + uniqueId;
    var confirmDeleteSpan = 'confirmDeleteSpan_' + uniqueId;

    if (isDeleteClicked) {
        $('#' + deleteSpan).hide();
        $('#' + confirmDeleteSpan).show();
    }
    else {
        $('#' + confirmDeleteSpan).hide();
        $('#' + deleteSpan).show();
    }
}