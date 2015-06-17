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

    $scope.SearchBookings = function (viewAll) {

        $("#bookingTable").html('');
        $("#bookingTable").html('<div class="loadingContainer"><img src="/Content/images/loading.gif" /></div>');

        var roomResults;

        var isValid = ValidateSearchFilters();
        if (isValid) {
            if (viewAll === true) {
                $scope.roomBookings = AllRooms.query({
                    location: $scope.bookingFilter.location.name,
                    startDate: FormatDateTimeForURL($scope.bookingFilter.startDate, 'MM-DD-YYYY', false)
                }, function (success) {
                    roomResults = success;
                    $scope.RenderBookings(roomResults);
                });
            }
            else {
                $scope.roomBookings = Available.query({
                    location: $scope.bookingFilter.location.name,
                    startDate: FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + $scope.bookingFilter.startTime, 'MM-DD-YYYY-HHmm', true),
                    endDate: FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + $scope.bookingFilter.endTime, 'MM-DD-YYYY-HHmm', true),
                    smartRoom: $scope.bookingFilter.smartRoom,
                    numberOfAttendees: $scope.bookingFilter.numberOfAttendees
                }, function (success) {
                    roomResults = success;
                    $scope.RenderBookings(roomResults);
                });
            }
        } else {
            $("#bookingTable").html('');
        }
        
    }

    $scope.RenderBookings = function (roomResults) {

        $("#bookingTable").html('');

        if (roomResults.length == 0) {
            $('#bookingTable').append('<div id="noMeetingsError" class="alert alert-danger alert-dismissible" role="alert">Sorry, there are no rooms available at the required time.</div>');
        }

        for (var i = 0; i < roomResults.length; i++) {
            var eventData = [];
            for (var b = 0; b < roomResults[i].bookings.length; b++) {
                eventData.push({
                    title: roomResults[i].bookings[b].owner,
                    start: roomResults[i].bookings[b].startDate,
                    end: roomResults[i].bookings[b].endDate
                });
            }

            if (roomResults[i].smartRoom === true) {
                var roomDetails = '<h3 style="text-align: center;">' +roomResults[i].roomName.replace('_', '.') + '<span title="Smart Room" id="smartroomBadgeIcon" class="badge"><span class="glyphicon glyphicon-facetime-video"></span></span></h3>';  
                }
            else {
                var roomDetails = '<h3 style="text-align: center;">' + roomResults[i].roomName.replace('_', '.') + '</h3>';
            }
                
            //debugger;

            roomDetails = roomDetails + '<span id="attendeesBadgeIcon" class="badge"><span class="glyphicon glyphicon-user" title="' + roomResults[i].seatCount + ' Attendees"> ' + roomResults[i].seatCount + '</span></span>';
            roomDetails = roomDetails + '<span id="pcBadgeIcon" class="badge"><span class="glyphicon glyphicon-hdd" title="' + roomResults[i].computerCount + ' PC\'s"> ' + roomResults[i].computerCount + '</span></span>';
            roomDetails = roomDetails + '<span id="phoneBadgeIcon" class="badge"><span class="glyphicon glyphicon-earphone" title="' + roomResults[i].phoneCount + ' Phones"> ' + roomResults[i].phoneCount + '</span></span>';

            $('#bookingTable').append('<div class="dailyCalendarContainer"><div id="roomDetailsBox" style="text-align: center;">' + roomDetails + '</div><div id="' + roomResults[i].roomName + '_calendar" class="dailyCalendar" title="Click and drag"></div></div>');
            var roomName = '[' + roomResults[i].roomName + ']';
            $("#" + roomResults[i].roomName + "_calendar").fullCalendar({
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
                timeFormat: "H:mm",
                axisFormat: "H:mm",
                allDaySlot: false,
                selectable: true,
                selectHelper: true,
                height: 452,
                titleFormat: '' + roomName + '',
                select: function (start, end, allDay) {

                    var newEvent = {
                        start: start, end: end
                    };
                    if (isMeetingOverlapping(newEvent, this.calendar.clientEvents())) {
                        alert('New meeting clashes with existing booking. Please choose a new time!');
                        return;
                    }

                    var $scope = angular.element($("#controllerDiv")).scope();
                    var room = $scope.GetRoomByName(this.title);

                    if (!room.smartRoom) {
                        $("#dssAssistChoice").css('display', 'none');
                    } else {
                        $("#dssAssistChoice").css('display', 'block');
                    }

                    $scope.newBooking.Room = room;
                    $scope.newBooking.Room.RoomName = room.roomName.replace('_', '.');

                    $scope.booking.StartTime = start.utc().format('H:mm');
                    $scope.booking.EndTime = end.utc().format('H:mm');

                    $scope.$digest();

                    $("#confirmModal #bookingModalStartTime").timepicker('setTime', start.utc().format('H:mm'));
                    $("#confirmModal #bookingModalEndTime").timepicker('setTime', end.utc().format('H:mm'));

                    $("#confirmModal").modal('show');                    
                },
                events: eventData
            });
        }
    }

    $('#activeDirecotryModal').on('show.bs.modal', function () {
        $scope.GetAdNames();
    });

    $scope.GetAdNames = function () {
        if ($scope.emailVal === undefined || $scope.emailVal.trim() === "") {
            $scope.adAdminUsers = Users.queryAll({
                allUsers: true
            })
        }
        else {
            $scope.adAdminUsers = Users.querySurname({
                allUsers: $scope.emailVal
            });
        }
    }

    $scope.AddAdEmail = function (adUser) {
        $scope.newBooking.Owner = adUser.firstName + ' ' + adUser.lastName;
        $scope.newBooking.PID = adUser.pid;
        $('#activeDirecotryModal').modal('hide');
    }

    $scope.AddExternalAttendee = function () {
        $scope.booking.ExternalNames = AddExternalName($scope.booking.ExternalNames);
    }

    $scope.RemoveExternalAttendee = function (fullName) {
        $scope.booking.ExternalNames = RemoveExternalName(fullName, $scope.booking.ExternalNames);
    }

    $scope.NewBooking = function () {
        SetModalErrorMessage(''); //Reset Any Error Messages

        //Change the "new booking" button to stop multiple bookings
        $("#newBookingConfirmButton").prop('disabled', 'disabled');
        $("#newBookingConfirmButton").html('Booking now. Please wait..');

        try {
            //Validate Number of External Names is not graether than attendees
            ValidateNoAttendees($scope.newBooking.Room.seatCount, $scope.booking.ExternalNames.length);

            //Validate Subject
            //ValidateSubject($scope.newBooking.Subject);

            var unsavedAttendee = ValidateUnSavedAttendee();
            if (unsavedAttendee !== "") {
                SetModalErrorMessage(unsavedAttendee);
                EnableNewBookingButton();
                return;
            }

            //Validate Time
            var timeValidation = ValidateStartEndTime($scope.booking.StartTime, $scope.booking.EndTime);
            if (timeValidation !== "") {
                SetModalErrorMessage(timeValidation);
                EnableNewBookingButton();
                return;
            }

            //Validate that new time does not clash
            var newEvent = {
                start: FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + $scope.booking.StartTime, null),
                end: FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + $scope.booking.EndTime, null)
            };

            if (isMeetingOverlapping(newEvent, $("#" + $scope.newBooking.Room.RoomName.replace('.', '_') + "_calendar").fullCalendar('clientEvents'))) {
                SetModalErrorMessage('New meeting clashes with existing booking. Please choose a new time!');
                EnableNewBookingButton();
                return;
            };

            $scope.newBooking.StartDate = FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + $scope.booking.StartTime, 'MM-DD-YYYY-HHmm',true);
            $scope.newBooking.EndDate = FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + $scope.booking.EndTime, 'MM-DD-YYYY-HHmm', true);
            $scope.newBooking.NumberOfAttendees = $scope.bookingFilter.numberOfAttendees;

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
                    EnableNewBookingButton();
                }
            });
        } catch (e) {
            EnableNewBookingButton();
        }
        
    }

    $('#confirmModal').on('show.bs.modal', function () {
        SetModalErrorMessage(''); //Reset Any Error Messages
    })

    $('#confirmModal').on('hidden.bs.modal', function () {
        $scope.newBooking.Subject = '',
        $scope.newBooking.Emails = '',
        $scope.booking.ExternalNames = [],
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
        StartTime: '',
        EndTime: '',
        ExternalNames: []
    };

    $scope.newBooking = {
        Room: { RoomName: '' },
        Subject: '',
        NumberOfAttendees: 1,
        ExternalNames: null,
        StartDate: new Date(),
        EndDate: new Date(),
        FlipChart: false,
        Projector: false,
        PID: '',
        Owner: '',
        DSSAssist: false
    };

    $scope.bookingFilter.endTime = IncrementCurrentTime(30);
}


function CreateServices($resource) {
    Locations = $resource('/api/locations', {
    }, {
        query: {
            method: 'GET', isArray: true
        }
    });

    Available = $resource('/api/availability/:location/:startDate/:endDate/:numberOfAttendees/:smartRoom', {
        location: 'location', startDate: 'startDate', endDate: 'endDate', numberOfAttendees: 'numberOfAttendees', smartRoom: 'smartRoom'
    },
    {
        query: {
            method: 'GET', isArray: true
        }
    });

    Available = $resource('/api/availability/:location/:startDate/:endDate/:numberOfAttendees/:smartRoom', {
        location: 'location', startDate: 'startDate', endDate: 'endDate', numberOfAttendees: 'numberOfAttendees', smartRoom: 'smartRoom'
    },
    {
        query: {
            method: 'GET', isArray: true
        }
    });

    AllRooms = $resource('/api/availability/:location/:startDate', {
        location: 'location', startDate: 'startDate'
    },
        {
            query: {
                method: 'GET', isArray: true
            }
        });

    Users = $resource('/api/users/:allUsers', {
        allUsers: 'allUsers'
    },
        {
            queryAll: {
                method: 'GET', isArray: true
            },
            querySurname: {
                method: 'GET', isArray: true
            }
        });
}


//Caladner UI Functions

function isMeetingOverlapping(event, array) {
    for (i in array) {
        if (event.end > array[i].start && event.start < array[i].end) {
            return true;
        }
    }
    return false;
}

function EnableNewBookingButton() {
    //Change the "new booking" button to stop multiple bookings
    $("#newBookingConfirmButton").prop('disabled', '');
    $("#newBookingConfirmButton").html('Confirm Booking');
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

function AdEmailExist(email, currentEmails) {
    for (var i = 0; i < currentEmails.length; i++) {
        if (email === currentEmails[i].toUpperCase().trim()) {
            return true;
        }
    }
}

function ValidateSearchFilters() {
    var valid = true;

    $("#searchFilterErrorList").html('');
    $("#searchFilterErrorCont").css('display', 'none');
    var errors = [];

    if ($("#searchFilter #searchLocation select")[0].selectedIndex == 0) {
        $("#searchFilter #searchLocation").addClass('has-error');
        errors.push('Must specify a location');
        valid = false;
    } else {
        $("#searchFilter #searchLocation").removeClass('has-error');
    }

    if ($("#searchFilter #attendeesInputFilter input").val().trim() == "") {
        $("#searchFilter #attendeesInputFilter").addClass('has-error');
        errors.push('Please enter number of attendees');
        valid = false;
    } else {
        $("#searchFilter #attendeesInputFilter").removeClass('has-error');
    }

    timeValidationMessage = ValidateStartEndTime($('#startTimePicker').val(), $('#endTimePicker').val())

    if (timeValidationMessage !== "") {
        $("#searchFilter #endTime").addClass('has-error');
        $("#searchFilter #startTime").addClass('has-error');
        errors.push(timeValidationMessage);
        valid = false;
    }
    else {
        $("#searchFilter #startTime").removeClass('has-error');
        $("#searchFilter #endTime").removeClass('has-error');
    }

    if (!valid) {
        for (var i = 0; i < errors.length; i++) {
            $("#searchFilterErrorList").append('<li>' + errors[i] + '</li>');
        }
        $("#searchFilterErrorCont").css('display', 'block');
    }
    return valid;
}

function IncrementCurrentTime(addMins) {
    var start = new moment();
    var diff = 30 - start.minute()

    if (diff > 0) {
        start.add(diff, 'm');
    }
    else {
        start.add(60 - start.minute(), 'm');
    }

   return moment(start).add("minutes", addMins).format("H:mm");
}

$(document).ready(function () {
    $("#onBehlafOfTextBox").keydown(function (e) {
        e.preventDefault();
    });
});
///////////////////////////////////////////////////////////////////
