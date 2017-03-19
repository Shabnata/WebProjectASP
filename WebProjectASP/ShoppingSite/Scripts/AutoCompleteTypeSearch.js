function typeSearch(elm) {
	var xhr = null;
	if (window.ActiveXObject) {
		xhr = new ActiveXObject("Microsoft.XMLHTTP");
	} else if (window.XMLHttpRequest) {
		xhr = new XMLHttpRequest();
	}

	if (xhr != null) {
		xhr.onreadystatechange = function () {
			if (this.readyState == 4 && this.status == 200) {
				var responseJSON = eval(this.responseText);
				$(elm).autocomplete({ source: responseJSON });
			}

		};

		xhr.open("POST", elm.getAttribute("path"), true);
		xhr.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
		xhr.send("SearchString=" + $(elm).val());
	} else {
		// DEBUGGING - PERMANENT
		console.log("xhr = null");
	}
}