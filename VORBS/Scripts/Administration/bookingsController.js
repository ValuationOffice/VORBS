administration.controller('MyBookingsController', ['$scope', '$http', '$resource', MyBookingsController]);

function MyBookingsController($scope, $http, $resource) {

    CreateBookingServices($resource);

    $scope.owners = Owner.getAll({});

    $scope.bookingId = 0;

    $scope.SetBookingId = function (id) {
        $scope.bookingId = id;
    }

    $scope.AddExternalAttendee = function () {
        SetModalErrorMessage('');
        $scope.booking.externalAttendees = AddExternalName($scope.booking.externalAttendees);
    }

    $scope.RemoveExternalAttendee = function (fullName) {
        SetModalErrorMessage('');
        $scope.booking.externalAttendees = RemoveExternalName(fullName, $scope.booking.externalAttendees);
    }

    $scope.externalFullNameTextBox = '';
    $scope.externalCompanyNameTextBox = '';
    $scope.externalPassRequired = false;

    $('#fullNameTextBox').typeahead({
        hint: false,
        highlight: true,
        minLength: 3
    },
    {
        name: 'owners',
        source: SubstringMatcher($scope.owners)
    });

    $scope.FormatPassRequired = function (required) {
        if (required) {
            return "Yes";
        }
        else {
            return "No";
        }
    }

    $scope.LoadEditBooking = function (bookingId) {
        //Reset Any Error Messages
        SetModalErrorMessage('');
        ResetExternalNamesUI();
        SetEditActiveTab('editBooking');

        $scope.SetBookingId(bookingId);

        $scope.editBooking = Booking.query({
            bookingId: $scope.bookingId
        },
        function (sucess) {
            $scope.newBooking.Room.RoomName = $scope.editBooking.room.roomName.replace('_', '.');
            $scope.newBooking.Subject = $scope.editBooking.subject;
            $scope.newBooking.FlipChart = $scope.editBooking.flipchart;
            $scope.newBooking.Projector = $scope.editBooking.projector;
            $scope.newBooking.RoomID = $scope.editBooking.room.id;

            if ($scope.editBooking.externalAttendees !== null) {
                $scope.booking.externalAttendees = $scope.editBooking.externalAttendees;//.split(';');
            }
            else {
                $scope.booking.externalAttendees = []; //Reset External Name List
            }

            $scope.booking.numberOfAttendees = $scope.editBooking.numberOfAttendees;
            $scope.booking.startTime = FormatTimeDate($scope.editBooking.startDate, false);
            $scope.booking.endTime = FormatTimeDate($scope.editBooking.endDate, false);
            $scope.booking.date = FormatTimeDate($scope.editBooking.startDate, true);

            //Store the vital existing booking data
            $scope.existingBooking.numberOfAttendees = $scope.booking.numberOfAttendees;
            $scope.existingBooking.startTime = $scope.booking.startTime;
            $scope.existingBooking.endTime = $scope.booking.endTime;
            $scope.existingBooking.date = $scope.booking.date;

            $scope.$apply();
            $("#editModal #bookingDate").datepicker('update');
        }
      )
    }

    $scope.CheckEditBooking = function () {
        //Reset Any Error Messages
        SetModalErrorMessage('');

        //Validate Start Date
        if ($scope.booking.date === "") {
            SetModalErrorMessage('Invalid date.');
            return;
        }

        //Validate Number of External Names is not graether than attendees
        ValidateNoAttendees($scope.booking.numberOfAttendees, $scope.booking.externalAttendees.length);
        $scope.newBooking.NumberOfAttendees = $scope.booking.numberOfAttendees;

        var unsavedAttendee = ValidateUnSavedAttendee();
        if (unsavedAttendee !== "") {
            SetModalErrorMessage(unsavedAttendee);
            return;
        }

        //Validate Times
        var timeValidation = ValidateStartEndTime($scope.booking.startTime, $scope.booking.endTime);
        if (timeValidation !== "") {
            SetModalErrorMessage(timeValidation);
            return;
        }

        //Change the "accept booking" button to stop multiple bookings
        $("#acceptBookingConfirmButton").prop('disabled', 'disabled');
        $("#acceptBookingConfirmButton").html('Editing Booking. Please wait..');

        try {
            //Create Date String
            $scope.newBooking.StartDate = FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.startTime, 'MM-DD-YYYY-HHmm', true, true);
            $scope.newBooking.EndDate = FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.endTime, 'MM-DD-YYYY-HHmm', true, true);

            if ($scope.booking.externalAttendees.length > 0) {
                //$scope.newBooking.externalAttendees = $scope.booking.ExternalNames.join(';');
                $scope.newBooking.externalAttendees = $scope.booking.externalAttendees;
            }

            //Validate if Date/Time/Attendees has changed
            if (($scope.booking.numberOfAttendees === $scope.existingBooking.numberOfAttendees) && ($scope.booking.date === $scope.existingBooking.date) &&
                ($scope.booking.startTime === $scope.existingBooking.startTime) && ($scope.booking.endTime === $scope.existingBooking.endTime)) {
                SaveEditBooking($scope.bookingId, $scope.newBooking);
            }
            else {
                $scope.availableRooms = Available.query({
                    location: $scope.editBooking.location.name,
                    startDate: FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.startTime, 'MM-DD-YYYY-HHmm', true, true),
                    endDate: FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.endTime, 'MM-DD-YYYY-HHmm', true, true),
                    smartRoom: false,
                    numberOfAttendees: $scope.booking.numberOfAttendees,
                    existingBookingId: $scope.bookingId
                },
                function (success) {
                    if ($scope.availableRooms.length === 0) {
                        EnableAcceptBookingButton();
                        SetModalErrorMessage('No rooms avaiLable using the below Date/Time/Attendees.');
                    }
                    else if ($scope.availableRooms[0].roomName.replace('_', '.') === $scope.newBooking.Room.RoomName) {
                        SaveEditBooking($scope.bookingId, $scope.newBooking);
                    }
                    else {
                        EnableAcceptBookingButton();
                        SetEditActiveTab('confirmEditBooking');
                        $scope.currentRoom = $scope.availableRooms[0];
                    }
                },
                function (error) {
                    EnableAcceptBookingButton();
                    alert('Unable to Edit. Please Try Again or Contact ITSD. ' + error.message);
                });
            }
        } catch (e) {
            EnableAcceptBookingButton();
        }
    }

    $scope.EditBooking = function () {
        //Change the "accept booking" button to stop multiple bookings
        $("#confirmBookingConfirmButton").prop('disabled', 'disabled');
        $("#confirmBookingConfirmButton").html('Editing Booking. Please wait..');

        $scope.newBooking.RoomID = $scope.currentRoom.id;
        SaveEditBooking($scope.bookingId, $scope.newBooking);

        EnableConfirmBookingButton();
    }

    $scope.DeleteBooking = function () {
        //Change the "delete booking" button to stop multiple bookings
        $("#deleteBookingConfirmButton").prop('disabled', 'disabled');
        $("#deleteBookingConfirmButton").html('Deleteing Booking. Please wait..');

        try {
            Booking.remove(
            {
                bookingId: $scope.bookingId
            },
            function (success) {
                $('#deleteModal').modal('hide');
                $scope.SearchBooking();
                EnableDeleteBookingButton();
            },
            function (error) {
                alert('Unable to Delete Booking. Please Try Again or Contact ITSD. ' + error.responseText);
                EnableDeleteBookingButton();
            });
        } catch (e) {
            EnableDeleteBookingButton();
        }
    }

    $scope.SearchBooking = function () {
        SetAdminBookingErrorMessage('');

        //Validate Full Name
        if ($('#fullNameTextBox').typeahead('val').trim() <= 0) {
            SetAdminBookingErrorMessage("Invalid full name.");
        }

        //Validate Date
        if ($('#startDatePicker').val() === "") {
            SetAdminBookingErrorMessage("Invalid booking date.");
        }

        $scope.bookings = GetBookings.query({
            owner: $('#fullNameTextBox').typeahead('val'),
            start: FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + "12:00", 'MM-DD-YYYY', true, true)
        },
        function (success) {
            if (success.length === 0) {
                SetAdminBookingErrorMessage('No bookings for ' + $('#fullNameTextBox').typeahead('val') + ' on the ' + $scope.bookingFilter.startDate + '.');
            }
        });
    }

    $scope.newBooking = {
        Room: { RoomName: '' },
        Subject: '',
        NumberOfAttendees: 0,
        RoomID: 0,
        externalAttendees: [],
        StartDate: new Date(),
        EndDate: new Date(),
        FlipChart: false,
        Projector: false
    };

    $scope.booking = {
        startTime: '',
        endTime: '',
        date: new Date(),
        numberOfAttendees: 0,
        externalAttendees: []
    }

    $scope.existingBooking = {
        startTime: '',
        endTime: '',
        date: new Date(),
        numberOfAttendees: 0
    }

    $scope.bookingFilter = {
        fullName: '',
        startDate: new moment().utc().format('DD-MM-YYYY')
    }
}

function CreateBookingServices($resource) {
    Available = $resource('/api/availability/:location/:startDate/:endDate/:numberOfAttendees/:smartRoom/:existingBookingId', {
        location: 'location', startDate: 'startDate', endDate: 'endDate', numberOfAttendees: 'numberOfAttendees', smartRoom: 'smartRoom', existingBookingId: 'existingBookingId'
    },
    {
        query: { method: 'GET', isArray: true }
    });

    Available.prototype = {
        roomNameFormatted: function () { return this.roomName.replace('_', '.'); }
    };

    GetBookings = $resource('/api/bookings/:owner/:start', { owner: 'owner', start: 'start' },
    {
        query: { method: 'GET', isArray: true }
    });

    GetBookings.prototype = {
        DateFormatted: function () { return moment(this.startDate).format("DD/MM/YYYY"); },
        startTimeFormatted: function () { return moment(this.startDate).format("H:mm"); },
        endTimeFormatted: function () { return moment(this.endDate).format("H:mm"); },
        roomNameFormatted: function () { return this.room.roomName.replace('_', '.'); }
    };

    Booking = $resource('/api/bookings/:bookingId', { bookingId: 'bookingId' },
    {
        query: { method: 'GET' },
        remove: { method: 'DELETE' }
    });

    Owner = $resource('/api/admin', {},
    {
        getAll: { method: 'GET', isArray: true }
    });
}

function SetAdminBookingErrorMessage(message) {
    if (message === "") {
        $('#adminBookingErrorMessage').hide();
    }
    else {
        $('#adminBookingErrorMessage').text(message);
        $('#adminBookingErrorMessage').show();
    }
}

function SubstringMatcher(strs) {
    return function findMatches(q, cb) {
        var matches, substringRegex;

        // an array that will be populated with substring matches
        matches = [];

        // regex used to determine if a string contains the substring `q`
        substrRegex = new RegExp(q, 'i');

        // iterate through the pool of strings and for any string that
        // contains the substring `q`, add it to the `matches` array
        $.each(strs, function (i, str) {
            if (substrRegex.test(str)) {
                matches.push(str);
            }
        });

        cb(matches);
    };
};