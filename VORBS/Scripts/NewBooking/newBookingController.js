newBooking.controller('NewBookingController', ['$scope', '$http', '$resource', NewBookingController]);

function NewBookingController($scope, $http, $resource) {
    CreateServices($resource);

    $scope.locations = Locations.query({ status: true, extraInfo: false });
    $scope.clashedBookings = [];

    $scope.externalFullNameTextBox = '';
    $scope.externalCompanyNameTextBox = '';
    $scope.externalPassRequired = false;
    //$scope.currentLocation = $scope.locations[0]


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
        IsSmartRoomChecked();
        $("#bookingTable").html('');
        $("#bookingTable").html('<div class="loadingContainer"><img src="/Content/images/loading.gif" /></div>');

        $("#newSearchResults").css('display', 'block');
        var roomResults;

        var advancedFilterVal = false;
        if (!viewAll) {
            advancedFilterVal = advancedSearchActive;
        }

        var isValid = ValidateSearchFilters(advancedFilterVal);
        if (isValid) {
            $("#searchedLocation").text($scope.bookingFilter.location.name);
            var formattedDate = new moment($scope.bookingFilter.startDate, ['DD-MM-YYYY']).format('dddd Do MMMM');
            $("#searchedDate").text(formattedDate);
            if (viewAll === true) {
                $scope.roomBookings = Available.query({
                    location: $scope.bookingFilter.location.name,
                    startDate: FormatDateTimeForURL($scope.bookingFilter.startDate, 'MM-DD-YYYY', false, true),
                    smartRoom: false
                }, function (success) {
                    roomResults = success;
                    $scope.RenderBookings(roomResults);
                });

            }
            else {

                var queryParams = {};
                if (advancedSearchActive) {
                    queryParams = {
                        location: $scope.bookingFilter.location.name,
                        startDate: FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + $scope.bookingFilter.startTime, 'MM-DD-YYYY-HHmm', true, true),
                        endDate: FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + $scope.bookingFilter.endTime, 'MM-DD-YYYY-HHmm', true, true),
                        smartRoom: $scope.bookingFilter.smartRoom,
                        numberOfAttendees: $scope.bookingFilter.numberOfAttendees
                    };
                } else {
                    queryParams = {
                        location: $scope.bookingFilter.location.name,
                        startDate: FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + $scope.bookingFilter.startTime, 'MM-DD-YYYY-HHmm', true, true),
                        smartRoom: $scope.bookingFilter.smartRoom
                    };
                }

                $scope.roomBookings = Available.query(queryParams, function (success) {
                    roomResults = success;
                    $scope.RenderBookings(roomResults);
                });
                $("#newSearchResults").css('display', 'block');
            }

            if ($scope.bookingFilter.smartRoom) {
                $scope.GetSmartLocations();
                $("#smartRoomLocations").css('display', 'block');
            }

        } else {
            $("#bookingTable").html('');
            $("#newSearchResults").css('display', 'none');
        }

    }

    $scope.RenderBookings = function (roomResults) {

        $("#bookingTable").html('');

        if (roomResults.length == 0) {
            $('#bookingTable').append('<div id="noMeetingsError" class="alert alert-danger alert-dismissible" role="alert">There are no meeting rooms that meet your requirements.</div>');
        }

        for (var i = 0; i < roomResults.length; i++) {
            var eventData = [];
            for (var b = 0; b < roomResults[i].bookings.length; b++) {
                eventData.push({
                    title: roomResults[i].bookings[b].owner,
                    start: roomResults[i].bookings[b].startDate,
                    end: roomResults[i].bookings[b].endDate,
                    isSmartMeeting: roomResults[i].bookings[b].isSmartMeeting
                });
            }

            //<span title="Smart Room" id="smartroomBadgeIcon" class="badge"><span class="glyphicon glyphicon-facetime-video"></span></span>

            var roomDetails = '<div class="calendarRoomName">' + roomResults[i].roomName + '</div>';

            //if (roomResults[i].smartRoom === true) {

            //}
            //else {
            //    var roomDetails = '<div class="calendarRoomName">' + roomResults[i].roomName + '</div>';
            //}

            roomDetails = roomDetails + '<div class="calendarRoomDetails">';
            if (roomResults[i].seatCount)
                roomDetails = roomDetails + '<span  class="glyphicon glyphicon-user calendarIcons" title="' + roomResults[i].seatCount + ' Attendees"></span>' + roomResults[i].seatCount;
            if (roomResults[i].computerCount)
                roomDetails = roomDetails + '<span  class="glyphicon glyphicon-hdd calendarIcons" title="' + roomResults[i].computerCount + ' PC(s)"></span>';
            if (roomResults[i].phoneCount)
            roomDetails = roomDetails + '<span  class="glyphicon glyphicon-earphone calendarIcons" title="' + roomResults[i].phoneCount + ' Phone(s)"></span>';

            if (roomResults[i].smartRoom === true) {
                roomDetails = roomDetails + '<span title="Smart Room" class="glyphicon glyphicon-facetime-video calendarIcons"></span>';
            }

            roomDetails = roomDetails + '</div>';

            $('#bookingTable').append('<div class="dailyCalendarContainer">' + roomDetails + '<div id="' + roomResults[i].roomName.replace('.', '_') + '_calendar" class="dailyCalendar" title="Click and drag"></div></div>');
            var roomName = '[' + roomResults[i].roomName + ']';
            $("#" + roomResults[i].roomName.replace('.', '_') + "_calendar").fullCalendar({
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
                eventBackgroundColor: 'rgba(33,132,110, 0.7)',
                eventTextColor: 'black',
                height: 600,
                titleFormat: '' + roomName + '',
                dayNames: ['', '', '', '', '', ''],
                select: function (start, end, allDay) {

                    var newEvent = {
                        start: start, end: end
                    };
                    if (isMeetingOverlapping(newEvent, this.calendar.clientEvents())) {
                        alert('New meeting clashes with existing booking. Please choose a new time!');
                        return;
                    }

                    var $scope = angular.element($("#controllerDiv")).scope();

                    ResetSearchFilterErrorList();

                    if (($("#smartLoactionDropDown")[0].selectedIndex > 0 || $scope.newBooking.SmartLoactions.length < 1) && $scope.bookingFilter.smartRoom) {
                        $("#searchFilter #smartLocationSelect").addClass('has-error');
                        var errors = 'Please select your other meeting location(s) and click the Add button';
                        $("#searchFilterErrorList").append('<li>' + errors + '</li>');
                        $("#searchFilterErrorCont").css('display', 'block');
                        return;
                    }

                    var room = Room.query({
                        locationId: $scope.bookingFilter.location.id,
                        roomName: this.title
                    }, function (success) {
                        if (!room.smartRoom || !$scope.bookingFilter.smartRoom) {
                            $("#dssAssistChoice").css('display', 'none');
                            $("#dssAssistContWarning").css("display", "none");
                        } else {
                            var dssDetails = GetLocationCredentialsFromList(dssCredentialsName, room.location.locationCredentials);
                            if (!dssDetails || dssDetails.email === "") {
                                $("#dssAssistChoice").css('display', 'none');
                                $("#dssAssistContWarning").css("display", "block");
                            } else {
                                $("#dssAssistChoice").css('display', 'block');
                                $("#dssAssistContWarning").css("display", "none");
                            }
                        }

                        var securityDetails = GetLocationCredentialsFromList(securityCredentialsName, room.location.locationCredentials);
                        if (!securityDetails || securityDetails.email === "") {
                            $("#externalAttendeesCont").css("display", "none");

                            var message = "This location does not have a dedicated security desk.";
                            var facilities = GetLocationCredentialsFromList(facilitiesCredentialsName, room.location.locationCredentials);
                            if (facilities) {
                                message = message + " Please contact the local facilities officer at " + facilities.email + " for visitory access protocols.";
                            }
                            $("#externalAttendeesContWarning").text(message);
                            $("#externalAttendeesContWarning").css("display", "block");
                        } else {
                            $("#externalAttendeesCont").css("display", "block");
                            $("#externalAttendeesContWarning").css("display", "none");
                        }

                        var facilitiesDetails = GetLocationCredentialsFromList(facilitiesCredentialsName, room.location.locationCredentials);
                        if (!facilitiesDetails || facilitiesDetails.email === "") {
                            $("#additionalEquipmentCont").css("display", "none");
                            $("#additionalEquipmentContWarning").css("display", "block");
                        } else {
                            $("#additionalEquipmentCont").css("display", "block");
                            $("#additionalEquipmentContWarning").css("display", "none");
                        }

                        $scope.newBooking.Room = room;
                        $scope.newBooking.Room.RoomName = room.roomName;
                    });

                    $scope.booking.StartTime = start.utc().format('H:mm');
                    $scope.booking.EndTime = end.utc().format('H:mm');

                    $scope.$digest();

                    $("#confirmModal #bookingModalStartTime").timepicker('setTime', start.utc().format('H:mm'));
                    $("#confirmModal #bookingModalEndTime").timepicker('setTime', end.utc().format('H:mm'));

                    $("#confirmModal").modal('show');
                },
                eventSources: [
                    {
                        events: eventData,
                        color: 'rgba(158, 158, 158, 0.7)',
                        textColor: 'black'
                    }
                ],
                eventRender: function (event, element) {
                    if (event.isSmartMeeting) {
                        $(element).prepend("<span class='glyphicon glyphicon-facetime-video' style='float:left;'></span>");
                    }
                }
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
        SetModalErrorMessage('');
        $scope.booking.ExternalAttendees = AddExternalName($scope.booking.ExternalAttendees);
        $("#externalAttendeesDisplay").css('display', 'block');

        $scope.externalFullNameTextBox = '';
        $scope.externalCompanyNameTextBox = '';
        $scope.externalPassRequired = false;
    }

    $scope.RemoveExternalAttendee = function (index) {
        SetModalErrorMessage('');
        $scope.booking.ExternalAttendees = RemoveExternalName(index, $scope.booking.ExternalAttendees);
        if ($scope.booking.ExternalAttendees.length === 0) {
            $("#externalAttendeesDisplay").css('display', 'none');
        }
    }

    $scope.GetSmartLocations = function () {
        if ($scope.bookingFilter.location === undefined || !$scope.bookingFilter.smartRoom) {
            return;
        }

        $scope.smartLoactions = [];

        for (var i = 0; i < $scope.locations.length; i++) {
            for (var k = 0; k < $scope.locations[i].rooms.length; k++) {
                if ($scope.locations[i].rooms[k].smartRoom && $scope.locations[i].rooms[k].active) {
                    $scope.smartLoactions.push({
                        displayName : $scope.locations[i].name + ' - ' +$scope.locations[i].rooms[k].roomName,
                        id: $scope.locations[i].rooms[k].id
                    });
                }
            }
        }

        for (var i = 0; i < $scope.newBooking.SmartLoactions.length; i++) {
            $scope.smartLoactions = $scope.RemoveLoaction($scope.newBooking.SmartLoactions[i], $scope.smartLoactions);
        }

        if ($scope.smartLoactions.indexOf($scope.bookingFilter.location.name) < 0) {
            $scope.newBooking.SmartLoactions = $scope.RemoveLoaction($scope.bookingFilter.location.name, $scope.newBooking.SmartLoactions);
        }
        else {
            $scope.smartLoactions = $scope.RemoveLoaction($scope.bookingFilter.location.name, $scope.smartLoactions);
        }
    }

    $scope.GetLocationById = function(id){
        for (var i = 0; i < $scope.smartLoactions.length; i++) {
            if ($scope.smartLoactions[i].id == id) {
                return $scope.smartLoactions[i];
            }
        }
    }

    $scope.GetFormattedSmartOptionTitle = function (roomId) {
        var roomData;
        for (var i = 0; i < $scope.locations.length; i++) {
            for (var r = 0; r < $scope.locations[i].rooms.length; r++) {
                if ($scope.locations[i].rooms[r].id == roomId) {
                    roomData = $scope.locations[i].rooms[r];
                }
            }
        }

        var formattedString = "";
        formattedString += roomData.seatCount + ' Seats, ';
        formattedString += roomData.computerCount + ' PC(s), ';
        formattedString += roomData.phoneCount + ' Phone(s)'
        return formattedString;
    }

    $scope.ShowSmartLoactions = function () {
        $scope.SearchBookings();
        if ($scope.bookingFilter.smartRoom) {
            $scope.GetSmartLocations();
            $("#smartRoomLocations").css('display', 'block');
        }
        else {
            $scope.newBooking.SmartLoactions = [];
            $("#smartRoomLocations").css('display', 'none');
        }
    }

    $scope.AddSmartLoaction = function () {
        ResetSearchFilterErrorList();
        if ($("#smartLoactionDropDown")[0].selectedIndex > 0) {
            $scope.newBooking.SmartLoactions.push($scope.currentSmartLocation);
            $scope.smartLoactions = $scope.RemoveLoaction($scope.currentSmartLocation, $scope.smartLoactions);
            $scope.currentSmartLocation = '';
            $("#smartLoactionDropDown")[0].selectedIndex = 0;
        }
        else {
            $("#searchFilter #smartLocationSelect").addClass('has-error');
            var errors = 'Please select your other meeting location(s) and click the Add button';
            $("#searchFilterErrorList").append('<li>' + errors + '</li>');
            $("#searchFilterErrorCont").css('display', 'block');
        }
    }

    $scope.ValidateSelection = function () {
        if ($("#smartLoactionDropDown")[0].selectedIndex > 0) {
            $("#searchFilter #smartLocationSelect").removeClass('has-error');
            $("#searchFilterErrorCont").css('display', 'none');
        }
    }

    $scope.RemoveSmartLoaction = function (locationName) {
        $scope.newBooking.SmartLoactions = $scope.RemoveLoaction(locationName, $scope.newBooking.SmartLoactions);
        $scope.smartLoactions.push(locationName);
        $scope.smartLoactions = $scope.smartLoactions.sort();
    }

    $scope.RemoveLoaction = function (locationName, locationArray) {
        for (var i = 0; i < locationArray.length; i++) {
            if (locationArray[i] === locationName) {
                locationArray.splice(i, 1);
                break;
            }
        }
        return locationArray;
    }

    $scope.NewBooking = function () {
        SetModalErrorMessage(''); //Reset Any Error Messages

        //Change the "new booking" button to stop multiple bookings
        $("#newBookingConfirmButton").prop('disabled', 'disabled');
        $("#newBookingConfirmButton").html('Booking now. Please wait..');

        try {
            //Validate Number of External Names is not graether than attendees
            ValidateNoAttendees($scope.newBooking.Room.seatCount, $scope.booking.ExternalAttendees.length);

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
                start: FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + $scope.booking.StartTime, null, true),
                end: FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + $scope.booking.EndTime, null, true)
            };

            if (isMeetingOverlapping(newEvent, $("#" + $scope.newBooking.Room.RoomName.replace('.', '_') + "_calendar").fullCalendar('clientEvents'))) {
                SetModalErrorMessage('New meeting clashes with existing booking. Please choose a new time!');
                EnableNewBookingButton();
                return;
            };

            $scope.newBooking.StartDate = FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + $scope.booking.StartTime, 'MM-DD-YYYY-HHmm', true, true);
            $scope.newBooking.EndDate = FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + $scope.booking.EndTime, 'MM-DD-YYYY-HHmm', true, true);
            $scope.newBooking.NumberOfAttendees = $scope.bookingFilter.numberOfAttendees;

            var originalRecurrenceEndDate = $scope.newBooking.Recurrence.EndDate;
            $scope.newBooking.Recurrence.EndDate = FormatDateTimeForURL($scope.newBooking.Recurrence.EndDate, 'MM-DD-YYYY-HHmm', true, false);

            if ($scope.booking.ExternalAttendees.length > 0) {
                $scope.newBooking.ExternalAttendees = $scope.booking.ExternalAttendees;//.join(';');
            }

            $.ajax({
                type: "POST",
                data: JSON.stringify($scope.newBooking),
                url: "api/bookings",
                contentType: "application/json",
                success: function (data, status) {
                    alert('Booking Confirmed.');
                    window.location.href = "/MyBookings"; //Redirect to my bookings
                },
                error: function (error) {
                    switch (error.status) {
                        case 409:
                            //conflict in recurrance booking(s)
                            $scope.clashedBookings = JSON.parse(JSON.parse(error.responseText).message);
                            $scope.$apply();
                            $("#meetingClashSelection").modal('show');
                            break;
                        case 502:
                            //no smart rooms avalible
                            $scope.clashedBookings = JSON.parse(JSON.parse(error.responseText).message);
                            $scope.$apply();
                            $("#smartMeetingClashSelection").modal('show');
                            break;
                        case 406:
                            //Server Conflict
                            alert("Simultaneous booking conflict. Please try again.");
                            $scope.SearchBookings(false);
                            $("#confirmModal").modal('hide');
                            break;
                        default:
                            alert('Unable to Book Meeting Room. ' + error.responseText);
                            break;
                    }
                    $scope.ResetConflictAction();
                    EnableNewBookingButton();
                }
            }
            );
            $scope.newBooking.Recurrence.EndDate = originalRecurrenceEndDate;
        } catch (e) {
            EnableNewBookingButton();
        }

    }

    $scope.ResetConflictAction = function () {
        $scope.newBooking.Recurrence.AutoAlternateRoom = false;
        $scope.newBooking.Recurrence.SkipClashes = false;
    }

    $scope.FormatDateToBritish = function (date) {
        return moment(date).format("DD/MM/YYYY");
    }

    $scope.FormatPassRequired = function (required) {
        if (required) {
            return "Yes";
        }
        else {
            return "No";
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
        $scope.ResetRecurrenceStatus();
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
        ExternalAttendees: []
    };

    $scope.newBooking = {
        Room: {
            RoomName: ''
        },
        Subject: '',
        NumberOfAttendees: 1,
        ExternalAttendees: [],
        StartDate: new Date(),
        EndDate: new Date(),
        FlipChart: false,
        Projector: false,
        PID: '',
        Owner: '',
        DSSAssist: false,
        SmartLoactions: [],
        Recurrence: {
            IsRecurring: false,
            SkipClashes: false,
            AutoAlternateRoom: false,
            AdminOverwrite: false,
            AdminOverwriteMessage: '',
            Frequency: 'daily',
            EndDate: '',
            DailyDayCount: 1,
            WeeklyWeekCount: 1,
            WeeklyDay: '',
            MonthlyMonthCount: 1,
            MonthlyMonthDayCount: 1,
            MonthlyMonthDay: 1
        }
    };

    //Added watch on frequency to update the recurrence settings section based on the current frequency value
    $scope.$watch('newBooking.Recurrence.Frequency', function (newValue, oldValue) {
        if (newValue != '' && newValue != null) {
            var sectionId = '';
            switch (newValue) {
                case 'daily':
                    sectionId = 'recDailySettings';
                    break;
                case 'weekly':
                    sectionId = 'recWeeklySettings';
                    break;
                case 'monthly':
                    sectionId = 'recMonthlySettings';
                    break;
            }

            $("#newBookingRecurrenceModal #recSettings .recSettingsPanel").css('display', 'none');
            $("#newBookingRecurrenceModal #recSettings #" + sectionId).css('display', 'block');
        }
    });

    $scope.CreateRecurrence = function () {

        if (ValidateRecurrenceSettings($scope.newBooking.Recurrence.Frequency)) {
            $scope.newBooking.Recurrence.IsRecurring = true;
            $scope.$apply();
            var breakDownText = '';
            switch ($scope.newBooking.Recurrence.Frequency) {
                case 'daily':
                    new moment().day(2).weekday
                    breakDownText = 'Occurs every ' + $scope.newBooking.Recurrence.DailyDayCount + ' day(s) effective ' + $scope.bookingFilter.startDate + ' until ' + $scope.newBooking.Recurrence.EndDate + ' from ' + $scope.booking.StartTime + ' to ' + $scope.booking.EndTime;
                    break;
                case 'weekly':
                    breakDownText = 'Occurs every ' + $scope.newBooking.Recurrence.WeeklyWeekCount + ' week(s) on ' + new moment().day($scope.newBooking.Recurrence.WeeklyDay).format('dddd') + ' effective ' + $scope.bookingFilter.startDate + ' until ' + $scope.newBooking.Recurrence.EndDate + ' from ' + $scope.booking.StartTime + ' to ' + $scope.booking.EndTime;;
                    break;
                case 'monthly':
                    var dayContext = $("#recMonthlyMonthDayCount option[value='" + $("#recMonthlyMonthDayCount").val() + "']").text()
                    var dayName = $("#recMonthlyMonthDay option[value='" + $("#recMonthlyMonthDay").val() + "']").text()
                    breakDownText = 'Occurs the ' + dayContext + ' ' + dayName + ' of every ' + $scope.newBooking.Recurrence.MonthlyMonthCount + ' month(s) effective ' + $scope.bookingFilter.startDate + ' until ' + $scope.newBooking.Recurrence.EndDate + ' from ' + $scope.booking.StartTime + ' to ' + $scope.booking.EndTime;;
                    break;
            }
            $("#recurringBreakDown").text(breakDownText);
            $("#newBookingRecurrenceModal").modal('hide');
        }
    }

    function ValidateRecurrenceSettings(frequency) {
        ClearRecurrenceErrors();
        valid = true;
        if ($("#recEndDate").val() == null || $("#recEndDate").val() == '') {
            valid = false;
            AddRecurrenceError("#newBookingRecurrenceModal #recEndDateCont", "Must specify a valid end date");
        }
        else if (FormatDateTimeForURL($("#recEndDate").val(), null, false, true) <= FormatDateTimeForURL($("#recStartDate").text(), null, false, true)) {
            valid = false;
            AddRecurrenceError("#newBookingRecurrenceModal #recEndDateCont", "Start date cannot be later than end date.");
        }

        switch (frequency) {
            case 'daily':
                if ($("#recDailyDayCount").val() == "" || $("#recDailyDayCount").val() == null || isNaN($("#recDailyDayCount").val())) {
                    valid = false;
                    AddRecurrenceError("#newBookingRecurrenceModal #recDailyDayCountCont", "Must specify a valid daily count");
                } else if ($("#recDailyDayCount").val() <= 0) {
                    valid = false;
                    AddRecurrenceError("#newBookingRecurrenceModal #recDailyDayCountCont", "Must specify a daily count greater than 0");
                }
                break;
            case 'weekly':
                if ($("#recWeeklyWeekCount").val() == "" || $("#recWeeklyWeekCount").val() == null || isNaN($("#recWeeklyWeekCount").val())) {
                    valid = false;
                    AddRecurrenceError("#newBookingRecurrenceModal #recWeeklyWeekCountCont", "Must specify a valid weekly count");
                } else if ($("#recWeeklyWeekCount").val() <= 0) {
                    valid = false;
                    AddRecurrenceError("#newBookingRecurrenceModal #recWeeklyWeekCountCont", "Must specify a weekly count greater than 0");
                }
                if ($scope.newBooking.Recurrence.WeeklyDay == null || $scope.newBooking.Recurrence.WeeklyDay == '' || isNaN($scope.newBooking.Recurrence.WeeklyDay)) {
                    valid = false;
                    AddRecurrenceError("#newBookingRecurrenceModal #recWeeklyWeekDay", "Must specify a day of the week");
                }
                break;
            case 'monthly':
                if ($("#recMonthlyMonthDayCount").val() == "" || $("#recMonthlyMonthDayCount").val() == null || isNaN($("#recMonthlyMonthDayCount").val())) {
                    valid = false;
                    AddRecurrenceError("#recMonthlySettings #recMonthlyMonthDayCountCont", "Must specify a monthly day count");
                }
                if ($("#recMonthlyMonthDay").val() == "" || $("#recMonthlyMonthDay").val() == null || isNaN($("#recMonthlyMonthDay").val())) {
                    valid = false;
                    AddRecurrenceError("#recMonthlySettings #recMonthlyMonthDay", "Must specify a monthly day");
                }
                if ($("#recMonthlyMonthCount").val() == "" || $("#recMonthlyMonthCount").val() == null || isNaN($("#recMonthlyMonthCount").val())) {
                    valid = false;
                    AddRecurrenceError("#recMonthlySettings #recMonthlyMonthCount", "Must specify a monthly count");
                }
                break;
        }
        return valid;
    }

    $scope.ResetRecurrenceStatus = function () {
        $scope.ResetRecurrenceObject();
        ClearRecurrenceErrors();

        $("#meetingClashSelection").modal('hide');
        $("#newBookingRecurrenceModal").modal('hide');

        $("#recurringBreakDown").text('');

    }

    $scope.CancelBookingClash = function () {
        $scope.ResetRecurrenceStatus();
    }

    $scope.ResetRecurrenceObject = function () {
        $scope.newBooking.Recurrence = {
            IsRecurring: false,
            SkipClashes: false,
            AutoAlternateRoom: false,
            AdminOverwrite: false,
            AdminOverwriteMessage: '',
            Frequency: 'daily',
            EndDate: '',
            DailyDayCount: 1,
            WeeklyWeekCount: 1,
            WeeklyDay: '',
            MonthlyMonthCount: 1,
            MonthlyMonthDayCount: 1,
            MonthlyMonthDay: 1
        };
    }

    $scope.CloseRecurrenceScreen = function () {
        ClearRecurrenceErrors();
    }

    $scope.SkipBookingClash = function () {
        $scope.newBooking.Recurrence.AutoAlternateRoom = false;
        $scope.newBooking.Recurrence.SkipClashes = true;
        $scope.newBooking.Recurrence.AdminOverwrite = false;

        $("#meetingClashSelection").modal('hide');

        $scope.NewBooking();
    }

    $scope.AlternateBookingClash = function () {
        $scope.newBooking.Recurrence.AutoAlternateRoom = true;
        $scope.newBooking.Recurrence.SkipClashes = false;
        $scope.newBooking.Recurrence.AdminOverwrite = false;

        $("#meetingClashSelection").modal('hide');

        $scope.NewBooking();
    }

    $scope.AdminOverwriteClash = function () {
        $scope.newBooking.Recurrence.AutoAlternateRoom = false;
        $scope.newBooking.Recurrence.SkipClashes = false;
        $scope.newBooking.Recurrence.AdminOverwrite = true;

        $("#meetingClashSelection").modal('hide');

        $scope.NewBooking();
    }

    $scope.bookingFilter.endTime = IncrementCurrentTime(30);

    $("#newBookingRecurrenceModal").on("show.bs.modal", function () {
        //$scope.newBooking.Recurrence.Frequency = 'daily';
        if ($scope.newBooking.Recurrence.WeeklyDay == '') {
            $scope.newBooking.Recurrence.WeeklyDay = new moment($scope.bookingFilter.startDate, ['DD-MM-YYYY']).day();
        }

        //$scope.newBooking.Recurrence.EndDate = $scope.bookingFilter.startDate;
        $scope.$apply();
    })
}


function CreateServices($resource) {
    Locations = $resource('/api/locations/:status/:extraInfo', { status: 'active', extraInfo: 'extraInfo' }, {
        query: { method: 'GET', isArray: true }
    });

    Available = $resource('/api/availability/:location/:startDate/:endDate/:numberOfAttendees/:smartRoom',
    {
        query: {
            method: 'GET', isArray: true
        }
    });

    Available = $resource('/api/availability/:location/:startDate/:endDate/:numberOfAttendees/:smartRoom',
    {
        query: {
            method: 'GET', isArray: true
        }
    });

    Room = $resource('/api/room/:locationId/:roomName', { locationId: 'locationId', roomName: 'roomName' }, {
        query: { method: 'GET', cache: false }
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

function ResetSearchFilterErrorList() {
    $("#searchFilterErrorList").html('');
    $("#searchFilterErrorCont").css('display', 'none');

    if ($("#smartRoomLocations").css('display') == 'block') {
        $("#searchFilter #smartLocationSelect").removeClass('has-error');
    }
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

function ValidateSearchFilters(advancedSearch) {
    var valid = true;

    ResetSearchFilterErrorList();
    var errors = [];

    if ($("#searchFilter #searchLocation select")[0].selectedIndex == 0) {
        $("#searchFilter #searchLocation").addClass('has-error');
        errors.push('Must specify a location');
        valid = false;
    } else {
        $("#searchFilter #searchLocation").removeClass('has-error');
    }

    if ($("#searchFilter #date input").val().trim() == "") {
        $("#searchFilter #date").addClass('has-error');
        errors.push('Please enter a date');
        valid = false;
    } else {
        $("#searchFilter #date").removeClass('has-error');
    }

    if (advancedSearch) {

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

        if ($("#searchFilter #attendeesInputFilter input").val().trim() == "") {
            $("#searchFilter #attendeesInputFilter").addClass('has-error');
            errors.push('Please enter number of attendees');
            valid = false;
        } else {
            $("#searchFilter #attendeesInputFilter").removeClass('has-error');
        }
    }

    if (!valid) {
        for (var i = 0; i < errors.length; i++) {
            $("#searchFilterErrorList").append('<li>' + errors[i] + '</li>');
        }
        $("#searchFilterErrorCont").css('display', 'block');
    }
    return valid;
}

function ClearRecurrenceErrors() {
    $("#recurrenceErrorCont").css('display', 'none');
    $("#recurrenceErrorList").html('');

    $("#newBookingRecurrenceModal .has-error").removeClass('has-error');
}

function AddRecurrenceError(elementSelector, errorMessage) {
    $(elementSelector).addClass('has-error');
    AddRecurrenceErrorMessage(errorMessage);
}

function AddRecurrenceErrorMessage(message) {
    var errorList = $("#recurrenceErrorList");
    if (errorList.children('li').length == 0) {
        $("#recurrenceErrorCont").css('display', 'block');
    }
    errorList.append('<li>' + message + '</li>');
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

function IsSmartRoomChecked() {    
    var message = "";
    message = (document.getElementById('smartRoomCheckBox').checked) ? "SMART" :  "";
    document.getElementById('isSmartRoom').innerText = message;
}

var advancedSearchActive = false;
function ToggleAdvancedSearch() {
    if (advancedSearchActive) {
        $("#advancedSearch").hide();
        $("#toggleAdvancedSearchLink").text('Advanced Search');
        $("#viewAllRoomsLink").hide();
    } else {
        $("#advancedSearch").show();
        $("#toggleAdvancedSearchLink").text('Hide Advanced Search');
        $("#viewAllRoomsLink").show();
    }
    advancedSearchActive = !advancedSearchActive;
}

$(document).ready(function () {
    $("#onBehlafOfTextBox").keydown(function (e) {
        if (e.key !== "Tab") {
            e.preventDefault();
        }
    });

    $('.touchSpinControl').TouchSpin({
        verticalbuttons: true,
        min: 1,
        initval: 1
    });
});
///////////////////////////////////////////////////////////////////
