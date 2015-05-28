newBooking.controller('NewBookingController', ['$scope', '$http', '$resource', NewBookingController]);

function NewBookingController($scope, $http, $resource) {
    CreateServices($resource);

    $scope.locations = Locations.query({});
    $scope.currentLocation = $scope.locations[0]


    //$scope.$watch('currentLocation', function () { $('#calendar').fullCalendar('refetchEvents') });
    //$scope.$watch('currentRoom', function () { $('#calendar').fullCalendar('refetchEvents') });
    //InitiateCalendar();

    $scope.GetRoomByName = function (roomName) {
        for (var i = 0; i < $scope.bookingFilter.location.rooms.length; i++) {
            if ($scope.bookingFilter.location.rooms[i].roomName == roomName) {
                return $scope.bookingFilter.location.rooms[i];
            }
        }
    }

    $scope.SearchBookings = function () {
        $scope.roomBookings = Available.query({
            location: $scope.bookingFilter.location.name,
            startDate: FormatDateForURL($scope.bookingFilter.startDate, false),
            endDate: FormatDateForURL($scope.bookingFilter.startDate, false),
            smartRoom: $scope.bookingFilter.smartRoom,
            numberOfAttendees: $scope.bookingFilter.numberOfAttendees
        }, function (success) {
            $("#bookingTable").html('');

            if (success.length == 0) {
                $('#bookingTable').append('<div id="noMeetingsError" class="alert alert-danger alert-dismissible" role="alert">Sorry, there are no rooms available at the required time.</div>');
            }

            for (var i = 0; i < success.length; i++) {
                var eventData = [];
                for (var b = 0; b < success[i].bookings.length; b++) {
                    eventData.push({
                        title: success[i].bookings[b].owner,
                        start: success[i].bookings[b].startDate,
                        end: success[i].bookings[b].endDate
                    });
                }
                var roomDetails = '<h2 style="text-align: center;">' + success[i].roomName + '</h2>';
                //debugger;
                roomDetails = roomDetails + '<span id="attendeesBadgeIcon" class="badge"><span class="glyphicon glyphicon-user" title="' + success[i].seatCount + ' Attendees"> ' + success[i].seatCount + '</span></span>';
                roomDetails = roomDetails + '<span id="pcBadgeIcon" class="badge"><span class="glyphicon glyphicon-hdd" title="' + success[i].computerCount + ' PC\'s"> ' + success[i].computerCount + '</span></span>';
                roomDetails = roomDetails + '<span id="phoneBadgeIcon" class="badge"><span class="glyphicon glyphicon-earphone" title="' + success[i].phoneCount + ' Phones"> ' + success[i].phoneCount + '</span></span>';                

                $('#bookingTable').append('<div class="dailyCalendarContainer"><div id="roomDetailsBox" style="text-align: center;">' + roomDetails + '</div><div id="' + success[i].roomName + '_calendar" class="dailyCalendar"></div></div>');
                var roomName = '[' + success[i].roomName + ']';
                $("#" + success[i].roomName + "_calendar").fullCalendar({
                    weekends: false,
                    header: {
                        left: '',
                        center: '',
                        right: ''
                    },
                    defaultDate: FormatDataForSearch($scope.bookingFilter.startDate),
                    defaultView: 'agendaDay',                    
                    minTime: "09:00:00",
                    maxTime: "17:30:00",
                    allDaySlot: false,
                    selectable: true,
                    selectHelper: true,
                    height: 452,
                    titleFormat: '' + roomName + '',
                    select: function (start, end, allDay) {

                        var newEvent = { start: start, end: end };
                        if (isMeetingOverlapping(newEvent, this.calendar)) {
                            alert('New meeting clashes with existing booking. Please choose a new time!');
                            return;
                        }

                        var $scope = angular.element($("#controllerDiv")).scope();
                        var room = $scope.GetRoomByName(this.title);

                        $scope.newBooking.Room.RoomName = room.roomName;
                        $scope.booking.StartDate = $scope.bookingFilter.startDate + ' ' + start.utc().format('HH:mm A');
                        $scope.booking.EndDate = $scope.bookingFilter.startDate + ' ' + end.utc().format('HH:mm A');

                        $scope.$digest();
                        $("#confirmModal").modal('show');
                    },
                    events: eventData
                });

            }

        });
    }

    $('#addressModal').on('show.bs.modal', function () {
        $scope.GetInternalEmails();
    });

    $scope.GetInternalEmails = function () {
        if ($scope.emailVal === undefined || $scope.emailVal.trim() === "") {
            $scope.interalPersons = Users.queryAll({
                allUsers: true
            })
        }
        else {
            $scope.interalPersons = Users.querySurname({
                allUsers: $scope.emailVal
            });
        }
    }

    $scope.AddAdEmail = function (email) {
        //Test to see if Email exists
        if (AdEmailExist(email.toUpperCase().trim(), $scope.newBooking.Emails.split(' '))) {
            alert('User Email Already Selected.');
            return;
        };

        //Tets to check if last char is space
        if (!(/\s+$/.test($scope.newBooking.Emails))) {
            $scope.newBooking.Emails += " ";
        }
        $scope.newBooking.Emails += email.trim();
        alert('User Added!');
    }

    $scope.AddExternalName = function () {

        var fName = $('#externalFirstNameTextBox').val();
        var lName = $('#externalLastNameTextBox').val();

        if (fName === null || lName === null) {
            //TODO: Validation - Duplicate - Null
        }

        $scope.booking.ExternalNames.push(fName + ' ' + lName);

        ResetExternalNamesUI();
    }

    $scope.RemoveExternalName = function (fullName) {
        for (var i = 0; i < $scope.booking.ExternalNames.length; i++) {
            if ($scope.booking.ExternalNames[i] === fullName) {
                $scope.booking.ExternalNames.splice(i, 1);
                break;
            }
        }
    }

    $scope.NewBooking = function () {
        //Reset Ant Error Message
        SetModalErrorMessage('');

        //Validate Emails
        var emails = $scope.newBooking.Emails.trim().split(' ');
        $scope.newBooking.Emails = ValidateEmails(emails);

        //Validate Number of External Names is not graether than attendees
        ValidateNoAttendees(emails.length, $scope.bookingFilter.numberOfAttendees, $scope.booking.ExternalNames.length);

        //Validate Subject
        var Subject = ValidateSubject($scope.newBooking.Subject);

        $scope.newBooking.StartDate = FormatDateForURL($scope.booking.StartDate, true);
        $scope.newBooking.EndDate = FormatDateForURL($scope.booking.EndDate, true);

        if ($scope.booking.ExternalNames.length > 0) {
            $scope.newBooking.ExternalNames = $scope.booking.ExternalNames.join(';');
        }

        $.ajax({
            type: "POST",
            data: JSON.stringify($scope.newBooking),
            url: "api/bookings",
            contentType: "application/json",

            success: function (data, status) {
                alert('Booking Confirmed. Meeting Requests Have Been Sent.');
                window.location.href = "/MyBookings"; //Redirect to my bookings
            },
            error: function (error) {
                alert('Unable to Book Meeting Room. Please Contact ITSD. ');
            }
        });
    }

    $('#confirmModal').on('hidden.bs.modal', function () {
        $scope.newBooking.Subject = '',
        $scope.newBooking.Emails = '',
        $scope.booking.ExternalNames = [],
        $scope.newBooking.Pc = false,
        $scope.newBooking.FlipChart = false,
        $scope.newBooking.Projector = false

        ResetExternalNamesUI();
    })

    $scope.bookingFilter = {
        startDate: new moment().utc().format('DD-MM-YYYY'),
        startTime: '',
        endTime: '',
        location: $scope.currentLocation,
        smartRoom: false,
        numberOfAttendees: 1
    };

    $scope.booking = {
        StartDate: new Date(),
        EndDate: new Date(),
        ExternalNames: []
    };

    $scope.newBooking = {
        Room: { RoomName: '' },
        Subject: '',
        Emails: '',
        ExternalNames: null,
        StartDate: new Date(),
        EndDate: new Date(),
        Pc: false,
        FlipChart: false,
        Projector: false
    };

    $scope.interalPersons = {
        Person: { name: '', emailAddress: '' }
    };
}


function CreateServices($resource) {
    Locations = $resource('/api/locations', {
    }, {
        query: { method: 'GET', isArray: true }
    });

    Available = $resource('/api/availability/:location/:startDate/:endDate/:numberOfAttendees/:smartRoom', { location: 'location', startDate: 'startDate', endDate: 'endDate', numberOfAttendees: 'numberOfAttendees', smartRoom: 'smartRoom' },
    {
        query: { method: 'GET', isArray: true }
    });

    Available = $resource('/api/availability/:location/:startDate/:endDate/:numberOfAttendees/:smartRoom', {
        location: 'location', startDate: 'startDate', endDate: 'endDate', numberOfAttendees: 'numberOfAttendees', smartRoom: 'smartRoom'
    },
    {
        query: {
            method: 'GET', isArray: true
        }
    });

    Users = $resource('/api/users/:allUsers', { allUsers: 'allUsers' },
        {
            queryAll: { method: 'GET', isArray: true },
            querySurname: { method: 'GET', isArray: true }
        });
}


//Caladner UI Functions

function isMeetingOverlapping(event, calendar) {
    var array = calendar.clientEvents();
    for (i in array) {
        if (event.end > array[i].start && event.start < array[i].end) {
            return true;
        }
    }
    return false;
}

function InitiateCalendar() {
    $(function () {
        $('#calendar').fullCalendar({
            weekends: false,
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'month,agendaWeek,agendaDay'
            },
            events: function (start, end, timezone, callback) {
                var $scope = angular.element($("#controllerDiv")).scope();
                $.ajax({
                    url: '/api/bookings/' + moment.utc(start, "DD-MM-YYYY").format("MM-DD-YYYY") + '/' + moment.utc(end, "DD-MM-YYYY").format("MM-DD-YYYY") + '/' + $scope.currentRoom.roomName,
                    dataType: 'json',
                    type: 'POST',
                    contentType: 'application/json; charset=utf-8',
                    data: JSON.stringify($scope.currentLocation),
                    success: function (data) {
                        var events = [];
                        for (var i = 0; i < data.length; i++) {
                            events.push({
                                title: data[i].owner,
                                start: data[i].startDate,
                                end: data[i].endDate
                            });
                        }
                        callback(events);
                    }
                });

            }
        });
    });
}
///////////////////////////////////////////////////////////////////

//Format Functions

function convertTo24Hour(time) {
    var hours = parseInt(time.substr(0, 2));
    time = time.toString().trim();
    if (time.indexOf('AM') != -1 && hours == 12) {
        time = time.replace('12', '0');
    }
    if (time.indexOf('PM') != -1 && hours < 12) {
        time = time.replace(hours, (hours + 12));
    }
    return time.replace(/(AM|PM)/, '').slice(0, -1);
}

function FormatDateForURL(date, hasTime) {

    if (date === "") {
        alert('Please Enter a Valid Date');
        throw new Error();
    }

    if (hasTime) {

        var timeDate = date.split(' ');

        var dateParts = timeDate[0].split('-');
        var timeParts = timeDate[1].split(':');

        myDate2 = new Date(dateParts[2], dateParts[1] - 1, dateParts[0]);
        myDate2.setHours(timeParts[0], timeParts[1], 0);

        return moment(myDate2).utc().format('MM-DD-YYYY-HHmm');
    }
    else {
        var parts = date.split('-');
        myDate2 = new Date(parts[2], parts[1] - 1, parts[0]);

        return moment(myDate2).utc().format('MM-DD-YYYY');
    }
}

function FormatDataForSearch(date) {
    var parts = date.split('-');
    return new Date(parts[2], parts[1] - 1, parts[0]);
}

function FormatTime(time, date) {
    if (time === "") {
        alert('Invalid Time');
        throw new Error();
    }

    var parts = date.split('-');
    var timeSplit = convertTo24Hour(time).split(':');

    if (timeSplit[0] > 17 || (timeSplit[0] == 17 && timeSplit[1] > 0)) {
        alert('Time Must Be Before 5.00 PM');
        throw new Error();
    }
    else if (timeSplit[0] < 9) {
        alert('Time Must Be After 9.00 AM');
        throw new Error();
    };


    var timeDate = new Date(parts[2], parts[1] - 1, parts[0]);
    timeDate.setHours(timeSplit[0], timeSplit[1], 0);

    return timeDate;
}
///////////////////////////////////////////////////////////////////

//New Booking Functions

function ValidateEmails(emails) {

    var attendeeEmails = "";

    if (emails.length < 1 || emails[0].trim() === "") {
        SetModalErrorMessage('No Emails Detected');
        throw new Error();
    }

    for (var i = 0; i < emails.length; i++) {
        //Validate Each Email
        if (emails[i].trim() === "" || !ValidateEmail(emails[i])) {
            SetModalErrorMessage('Invalid Email Detected: ' + emails[i]);
            throw new Error();
        }
        else {
            attendeeEmails += emails[i].trim() + ";"
        }
    }

    return attendeeEmails;
}

function ValidateNoAttendees(attendees, filterAttendees, externalNamesLength) {
    if (externalNamesLength > attendees) {
        SetModalErrorMessage('Too Many External Attendees for Meeting Room');
        throw new Error();
    }
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

function SetModalErrorMessage(message) {
    if (message === "") {
        $('#newBookingErrorMessage').hide();
    }
    else {
        $('#newBookingErrorMessage').text(message);
        $('#newBookingErrorMessage').show();
    }
}

function ResetExternalNamesUI() {
    $('#externalFirstNameTextBox').val('');
    $('#externalLastNameTextBox').val('');
}

function AdEmailExist(email, currentEmails) {
    for (var i = 0; i < currentEmails.length; i++) {
        if (email === currentEmails[i].toUpperCase().trim()) {
            return true;
        }
    }
}

///////////////////////////////////////////////////////////////////

//Filtering Functions

function ValidateAttendess(attendees) {

    if (attendees === null) {
        throw new Error();
    }
    else if (isNaN(attendees)) {
        alert('Invalid Number');
        throw new Error();
    }
    else if (attendees <= 0) {
        alert('Attendees Can Not Be Negative');
        throw new Error();
    }
    return attendees;
}

function ValidateDates(start, end) {
    //ToDo: Add Equals then
    if (start > end) {
        alert('Start Date Can Not Be Ahead Of End Date');
        throw new Error();
    }
}

function IncrementTime(time, minutes) {
    var timeSplit = time.split(':');
    var amPm = timeSplit[1].split(" ");

    minutes = parseInt(amPm[0]) + minutes

    if (minutes >= 60) {
        minutes -= 60;
        ("0" + minutes).slice(-2);
    }

    return "'" + timeSplit[0] + ":" + minutes + " " + amPm[1] + "'";
}
///////////////////////////////////////////////////////////////////
