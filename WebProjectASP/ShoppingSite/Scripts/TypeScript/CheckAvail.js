function setState(available, fieldID) {
    if (available === void 0) { available = false; }
    var checkField = document.getElementById(fieldID);
    var glyphiconOK = document.getElementById("glyphiconOK");
    var glyphiconRem = document.getElementById("glyphiconRem");
    var submitButton = document.getElementById("submitButton");
    if (checkField.value.length == 0) {
        //console.log("setState() : value.length == 0");
        glyphiconOK.style.display = "none";
        glyphiconRem.style.dispaly = "none";
        submitButton.disabled = true;
        return;
    }
    if (available) {
        //console.log("setState() : available == true");
        glyphiconOK.style.display = "block";
        glyphiconRem.style.display = "none";
        submitButton.disabled = false;
    }
    else {
        //console.log("setState() : available == false");
        glyphiconOK.style.display = "none";
        glyphiconRem.style.display = "block";
        submitButton.disabled = true;
    }
}
function checkAvailability(url, id, fieldID) {
    if (id === void 0) { id = "-1"; }
    //console.log(url + ", " + id + "," + fieldID);
    var checkField = document.getElementById(fieldID);
    //console.log("checkAvailability : fieldID , checkField.value = " + fieldID + ", " + checkField.value);
    if (checkField.value.length == 0) {
        //console.log("checkAvailable() : value.length == 0");
        setState(false, fieldID);
    }
    else {
        var xhr = new XMLHttpRequest();
        xhr.onreadystatechange = function () {
            //console.log("onreadystatechange() : (readyState, status) = (" + xhr.readyState + ", " + xhr.status + ")");
            if (xhr.readyState == 4 && xhr.status == 200) {
                //console.log("onreadystatechange() : responseText = " + xhr.responseText);
                setState(eval(xhr.responseText), fieldID);
            }
        };
        var params = new FormData();
        params.append("Name", checkField.value);
        params.append("ID", id);
        //console.log("checkAvailable() : openning and sending");
        xhr.open("POST", url, true);
        xhr.send(params);
    }
}
//# sourceMappingURL=CheckAvail.js.map