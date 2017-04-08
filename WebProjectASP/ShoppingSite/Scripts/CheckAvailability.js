function CheckAvailability(e) {

	if ($(this).val().length == 0) {
		$(".glyphicon-ok").hide();
		$(".glyphicon-remove").hide();
	} else {
		$.ajax({
			type: "POST",
			url: e.data.url,
			data: {
				Name: $("#" + e.data.name).val(),
				ID: e.data.id
			}
		}).done(function (data, status) {
			console.log(data);
			var responseJSON = eval(data);
			if (responseJSON == true) {
				$(".glyphicon-ok").show();
				$(".glyphicon-remove").hide();
				$("input[type='submit']").attr("disabled", false);
			} else {
				$(".glyphicon-ok").hide();
				$(".glyphicon-remove").show();
				$("input[type='submit']").attr("disabled", true);
			}
		}).fail(function (xmlHttpRequest, statusText, errorThrown) {
			alert(
			'Your form submission failed.\n\n'
			+ 'XML Http Request: ' + JSON.stringify(xmlHttpRequest)
			+ ',\nStatus Text: ' + statusText
			+ ',\nError Thrown: ' + errorThrown);
		});
	}
}
