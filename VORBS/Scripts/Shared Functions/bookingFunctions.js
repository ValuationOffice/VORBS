
//Modal Functions
function SetModalErrorMessage(message) {
    if (message === "") {
        $('#bookingModalErrorMessage').hide();
    }
    else {
        $('#bookingModalErrorMessage').text(message);
        $('#bookingModalErrorMessage').show();
    }
}

function ResetExternalNamesUI() {
    $('#externalFirstNameTextBox').val('');
    $('#externalLastNameTextBox').val('');
}

function AddExternalName(attendeeArray) {
    var fName = $('#externalFirstNameTextBox').val();
    var lName = $('#externalLastNameTextBox').val();

    if (fName.trim() === "" || lName.trim() === "") {
        SetModalErrorMessage("Invalid external attendee.");
    }
    else {
        var fullName = fName + ' ' + lName;
        if (attendeeArray.indexOf(fullName) <= -1) {
            attendeeArray.push(fullName);
        }
        ResetExternalNamesUI();
    }
    return attendeeArray;
}

function RemoveExternalName(fullName, attendeeArray) {
    for (var i = 0; i < attendeeArray.length; i++) {
        if (attendeeArray[i] === fullName) {
            attendeeArray.splice(i, 1);
            break;
        }
    }
    return attendeeArray;
}

function ValidateNoAttendees(filterAttendees, externalNamesLength) {
    if (filterAttendees === null) {
        SetModalErrorMessage('Invalid number of attendees.');
        throw new Error();
    }
    if (externalNamesLength > filterAttendees) {
        SetModalErrorMessage('The number of attendees for this meeting exceeds the room capacity. Please search for a suitable meeting room and book again.');
        throw new Error();
    }
}

function ValidateSubject(subject) {
    if (subject.trim() === "") {
        SetModalErrorMessage('Invalid subject detected.');
        throw new Error();
    }
    return subject;
}

function SetEditActiveTab(tabId) {
    $('#editBookingTabs a[href="#' + tabId + '"]').tab('show');
}

function SetAdminActiveTab(tabId) {
    $('#adminTabs a[href="#' + tabId + '"]').tab('show');
}


function SaveEditBooking(existingId, editBooking) {
    try {
        $.ajax({
            type: "POST",
            data: JSON.stringify(editBooking),
            url: "api/bookings/" + existingId,
            contentType: "application/json",
            success: function (data, status) {
                alert('Edit booking confimed. Confirmation email has beeen sent.');
                ReloadThisPage("bookings");
            },
            error: function (error) {
                EnableEditBookingButton();
                alert('Unable to edit meeting room. Please contact ITSD. ' + error.message);
            }
        });
    } catch (e) {
        EnableEditBookingButton();
    }
}

function EnableAcceptBookingButton() {
    //Change the "new booking" button to stop multiple bookings
    $("#acceptBookingConfirmButton").prop('disabled', '');
    $("#acceptBookingConfirmButton").html('Accept');
}

function EnableConfirmBookingButton() {
    //Change the "new booking" button to stop multiple bookings
    $("#confirmBookingConfirmButton").prop('disabled', '');
    $("#confirmBookingConfirmButton").html('Confirm');
}

function EnableDeleteBookingButton() {
    //Change the "delete booking" button to stop multiple bookings
    $("#deleteBookingConfirmButton").prop('disabled', '');
    $("#deleteBookingConfirmButton").html('Delete');
}

function EnableEditBookingButton() {
    //Change the "edit booking" button to stop multiple bookings
    $("#acceptBookingConfirmButton").prop('disabled', '');
    $("#acceptBookingConfirmButton").html('Accept');
}



////////////////

//Validation
function ValidateStartEndTime(start, end) {

    if (start === "" || end === "") {
        return "Invalid start/end time.";
    }

    var timeSplit = start.split(':');
    var startDate = new moment().hour(timeSplit[0]).minute(timeSplit[1]);

    timeSplit = end.split(':');
    var endDate = new moment().hour(timeSplit[0]).minute(timeSplit[1]);

    var timeDiff = endDate.diff(startDate, 'm');

    if (startDate.minutes() !== 0 && startDate.minutes() !== 30) {
        return "Start time must be 30 minute interval."
    }
    else if (endDate.minutes() !== 0 && endDate.minutes() !== 30) {
        return "End time must be 30 minute interval."
    }
    else if (startDate.hours() < 9 || startDate.hours() >= 18) {
        return "Start time has to be between 9:00 & 17:30.";
    }
    else if (endDate.hours() < 9 || endDate.hours() >= 18) {
        return "End time has to be between 9:00 & 17:30.";
    }
    else if (timeDiff < 0) {
        return "Start time can't be ahead of end time.";
    }
    else if (timeDiff === 0) {
        return "Identical times can't be used as a valid search.";
    }

    return "";
}


function ValidateEmail(email) {
    var regex = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
    return regex.test(email);
}

function ValidateUnSavedAttendee() {
    if ($('#externalFirstNameTextBox').val().trim() !== "" || $('#externalLastNameTextBox').val().trim() !== "") {
        return "Unsaved external attendee. Please press add to save external attendee.";
    }
    return "";
}


////////////////

//Formating (Date/Time)
function FormatDateTimeForURL(date, formatString, toUtc) {

    date = date.trim();
    if (date === "") {
        alert('Please Enter a Valid Date');
        throw new Error();
    }

    var timeDate = date.split(' ');

    var dateParts = timeDate[0].split('-');
    myDate2 = new Date(dateParts[2], dateParts[1] - 1, dateParts[0]);

    //If date incudes time add time
    if (timeDate.length > 1) {
        var timeParts = timeDate[1].split(':');
        myDate2.setHours(timeParts[0], timeParts[1], 0);
    }

    if (formatString !== null) {
        if (toUtc) {
            return new moment(myDate2).utc().format(formatString);
        }
        return new moment(myDate2).format(formatString);
    }
    else {
        //Workaround for confirmation edit time 
        return new moment(myDate2).add(1, 'h');
    }
}

function FormatTimeDate(dateTime, returnDate) {
    if (dateTime === "") {
        alert('Please Enter a Valid Date');
        throw new Error();
    }

    if (returnDate) {
        return new moment(dateTime).format('DD-MM-YYYY');
    }
    else {
        return new moment(dateTime).format('H:mm');
    }
}

////////////////

//Attendees Control Functions

$(document).ready(function () {
    $(".phoneNoControl,.numberControl").keydown(function (e) {
        // Allow: backspace, delete, tab, escape, enter and .
        if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110]) !== -1 ||
            // Allow: Ctrl+A, Command+A
            (e.keyCode == 65 && (e.ctrlKey === true || e.metaKey === true)) ||
            // Allow: home, end, left, right, down, up
            (e.keyCode >= 35 && e.keyCode <= 40)) {
            // let it happen, don't do anything
            return;
        }
        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
            e.preventDefault();
        }
    });

    $("#roomName").keydown(function (e) {
        // Allow: backspace, delete, tab, escape, enter and .
        if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110]) !== -1 ||
            // Allow: Ctrl+A, Command+A
            (e.keyCode == 65 && (e.ctrlKey === true || e.metaKey === true)) ||
            // Allow: home, end, left, right, down, up
            (e.keyCode >= 35 && e.keyCode <= 40)) {
            // let it happen, don't do anything
            return;
        }

        // Ensure that only numbers and letters are allowed
        if (!((e.keyCode >= 48 && e.keyCode <= 105) || e.keyCode === 190)) {
            e.preventDefault();
        }
    });
})


////////////////

//Admin Functions

function ReloadThisPage(tabId) {
    var hashIndex = window.location.href.indexOf("#");
    if (hashIndex > 0) {
        window.location.href = window.location.href.substring(0, hashIndex) + "#" + tabId;
    }
    else {
        window.location.href = window.location.href + "#" + tabId;
    }

    location.reload();
}

////////////////