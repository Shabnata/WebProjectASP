function formatDatePicker(elmID) {
	var elm = $("#" + elmID);
	var dateStr = elm.val();
	var formattedDate = dateStr.split(" ")[0];

	$(elm).val(formattedDate);
}