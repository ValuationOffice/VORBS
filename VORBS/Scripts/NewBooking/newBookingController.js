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
            startDate: FormatDateForURL($scope.bookingFilter.startDate),
            endDate: FormatDateForURL($scope.bookingFilter.startDate),
            smartRoom: $scope.bookingFilter.smartRoom,
            numberOfAttendees: $scope.bookingFilter.numberOfAttendees
        }, function (success) {
            $("#bookingTable").html('');

            for (var i = 0; i < success.length; i++) {
                var eventData = [];
                for (var b = 0; b < success[i].bookings.length; b++) {
                    eventData.push({
                        title: success[i].bookings[b].owner,
                        start: new moment(success[i].bookings[b].startTime),
                        end: new moment(success[i].bookings[b].endTime)
                    });
                }
                var roomDetails = '<h2 style="text-align: center;">' + success[i].roomName + '</h2>';
                //debugger;
                roomDetails = roomDetails + '<span class="badge"><span class="glyphicon glyphicon-hdd" title="' + success[i].computerCount + ' PC\'s"> ' + success[i].computerCount + '</span></span>';
                roomDetails = roomDetails + '<span class="badge"><span class="glyphicon glyphicon-earphone" title="' + success[i].phoneCount + ' Phones"> ' + success[i].phoneCount + '</span></span>';

                $('#bookingTable').append('<div class="dailyCalendarContainer"><div style="text-align: center;">' + roomDetails + '</div><div id="' + success[i].roomName + '_calendar" class="dailyCalendar"></div></div>');
                var roomName = '[' + success[i].roomName + ']';
                $("#" + success[i].roomName + "_calendar").fullCalendar({
                    weekends: false,
                    header: {
                        left: '',
                        center: '',
                        right: ''
                    },
                    //defaultDate: $scope.bookingFilter.startDate,                    
                    defaultView: 'agendaDay',
                    selectable: true,
                    selectHelper: true,
                    height: 500,
                    titleFormat: '' + roomName + '',
                    select: function (start, end, allDay) {

                        var newEvent = { start: start, end: end };
                        if (isMeetingOverlapping(newEvent, this.calendar)) {
                            alert('New meeting clashes with existing booking. Please choose a new time!"');
                            return;
                        }

                        var $scope = angular.element($("#controllerDiv")).scope();
                        var room = $scope.GetRoomByName(this.title);

                        $scope.newBooking.RoomName = room.roomName;

                        $scope.$digest();
                        $("#confirmModal").modal('show');
                    },
                    events: eventData
                });

            }

        });
    }

    $scope.AddExternalName = function () {

        var fName = $('#externalFirstNameTextBox').val();
        var lName = $('#externalLastNameTextBox').val();

        if (fName === null || lName === null) {
            //TODO: Validation - Duplicate - Null
        }

        $scope.newBooking.ExternalNames.push({
            fName: fName,
            lName: lName
        });

        $('#externalFirstNameTextBox').val('');
        $('#externalLastNameTextBox').val('');
    }

    $scope.RemoveExternalName = function (fName, lName) {
        for (var i = 0; i < $scope.newBooking.ExternalNames.length; i++) {
            if ($scope.newBooking.ExternalNames[i].fName === fName && $scope.newBooking.ExternalNames[i].lName === lName) {
                $scope.newBooking.ExternalNames.splice(i, 1);
                break;
            }
        }
    }

    $scope.NewBooking = function () {

        //Validate Emails
        var emails = $scope.newBooking.AttendeeEmails.split(' ');
        var AttendeeEmails = ValidateEmails(emails);

        //Validate Number Of Emails Matches Attendees 
        //Validate Number of External Names is not graether than attendees
        ValidateNoAttendees(emails.length, $scope.bookingFilter.numberOfAttendees, $scope.newBooking.ExternalNames.length);

        //Validate Subject
        var Subject = ValidateSubject($scope.newBooking.Subject);

        Booking.save({
            room: $scope.newBooking.RoomName,
            startDate: $scope.newBooking.StartDate,
            endDate: $scope.newBooking.EndDate,
            subject: Subject,
            attendeeEmails: Emails,
            externalNames: JSON.stringify($scope.newBooking.ExternalNames),
            pc: $scope.newBooking.Pc,
            flipchart: $scope.newBooking.FlipChart,
            projector: $scope.newBooking.Projector
        },
        function (success) {
            alert('Booking Saved');
        },
        function (error) {
            alert('Error');
        })

    }

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
        Room: { RoomName: '' },
        Location: { LocationName: '' }
    };

    $scope.newBooking = {
        RoomName: '',
        Subject: '',
        AttendeeEmails: '',
        ExternalNames: [],
        StartDate: new moment().format("DD-MM-YYYY HH:mm A"), //Change To Chart Input
        EndDate: new moment().format("DD-MM-YYYY HH:mm A"), //Change To Chart Input
        Pc: false,
        FlipChart: false,
        Projector: false
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

    Booking = $resource('/api/bookings/:room/:startDate/:endDate/:subject/:attendeeEmails/:externalNames/:pc/:flipchart/:projector', { room: 'room', startDate: 'startDate', endDate: 'endDate', subject: 'subject', attendeeEmails: 'attendeeEmails', externalNames: 'externalNames', pc: 'pc', flipchart: 'flipchart', projector: 'projector' },
    {
        save: { method: 'POST' }
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

function FormatDateForURL(date) {

    if (date === "") {
        alert('Please Enter a Valid Date');
        throw new Error();
    }

    var parts = date.split('-');
    myDate2 = new Date(parts[2], parts[1] - 1, parts[0]);

    return moment(myDate2).format('MM-DD-YYYY');
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

    var attendeeEmails = null;

    if (emails.length < 1) {
        alert('No Emails Detected');
        throw new Error();
    }

    for (var i = 0; i < emails.length; i++) {
        //Validate Each Email
        if (emails[i].trim() === "" || !ValidateEmail(emails[i])) {
            alert('Invalid Email Detected: ' + emails[i]);
            throw new Error();
        }
        else {
            attendeeEmails += emails[i].trim() + ";"
        }
    }

    return attendeeEmails;
}

function ValidateNoAttendees(attendees, filterAttendees, externalNamesLength) {
    if (attendees !== filterAttendees) {
        alert('Number of Atteneds Does Not Match Number of Emails.');
        throw new Error();
    }
    else if (externalNamesLength > attendees) {
        alert('Too Many External names');
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
