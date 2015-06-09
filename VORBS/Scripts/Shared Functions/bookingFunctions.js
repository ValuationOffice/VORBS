
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
        SetModalErrorMessage("Invalid External Attendee !");
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

////////////////

//Validation
function ValidateNoAttendees(filterAttendees, externalNamesLength) {
    if (filterAttendees === null) {
        SetModalErrorMessage('Invalid Number Of Attendees.');
        throw new Error();
    }
    if (externalNamesLength > filterAttendees) {
        SetModalErrorMessage('The Number of Attendees for this Meeting Exceeds the Room Capacity.');
        throw new Error();
    }
}

function ValidateStartEndTime(start, end) {

    var timeSplit = start.split(':');
    var startDate = new moment().hour(timeSplit[0]).minute(timeSplit[1]).add(30, 'm');

    timeSplit = end.split(':');
    var endDate = new moment().hour(timeSplit[0]).minute(timeSplit[1]);

    if (startDate.isAfter(endDate)) {
        return "Start Time cannont be ahead of End Time";
    }

    return "";
}

function ValidateSubject(subject) {
    if (subject.trim() === "") {
        SetModalErrorMessage('Invalid Subject Detected');
        throw new Error();
    }
    return subject;
}

function ValidateEmail(email) {
    var regex = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
    return regex.test(email);
}

////////////////

//Formating (Date/Time)
function FormatDateTimeForURL(date, formatString) {
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
        return new moment(myDate2).utc().format(formatString);
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
    $('#attendeesControl').TouchSpin({
        verticalbuttons: true,
        min: 1,
        initval: 1
    });

    $("#attendeesControl").keydown(function (e) {
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
})


////////////////