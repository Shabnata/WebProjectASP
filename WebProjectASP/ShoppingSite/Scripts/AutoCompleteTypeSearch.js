function typeSearch(elm) {
	var url = $(this).attr("path");
	console.log(url);
	$.ajax({
		type: "POST",
		url: url,
		caller: $(this).attr("id"),
		data: {
			SearchString: $(this).val()
		},
		success: function (data, status) {
			var responseJSON = eval(data);
			$("#" + this.caller).autocomplete({ source: responseJSON });
		},
		error: function (xmlHttpRequest, statusText, errorThrown) {
			alert(
			'Your form submission failed.\n\n'
			+ 'XML Http Request: ' + JSON.stringify(xmlHttpRequest)
			+ ',\nStatus Text: ' + statusText
			+ ',\nError Thrown: ' + errorThrown);
		}
	});
}

/*
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
*/