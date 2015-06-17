administration.controller('MyBookingsController', ['$scope', '$http', '$resource', MyBookingsController]);

function MyBookingsController($scope, $http, $resource) {

    CreateBookingServices($resource);

    $scope.owners = Owner.getAll({});

    $scope.bookingId = 0;

    $scope.SetBookingId = function (id) {
        $scope.bookingId = id;
    }

    $scope.AddExternalAttendee = function () {
        $scope.booking.ExternalNames = AddExternalName($scope.booking.ExternalNames);
    }

    $scope.RemoveExternalAttendee = function (fullName) {
        $scope.booking.ExternalNames = RemoveExternalName(fullName, $scope.booking.ExternalNames);
    }

    $('#fullNameTextBox').typeahead({
        hint: true,
        highlight: true,
        minLength: 1
    },
    {
        name: 'owners',
        source: SubstringMatcher($scope.owners)
    });

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

            if ($scope.editBooking.externalNames !== null) {
                $scope.booking.ExternalNames = $scope.editBooking.externalNames.split(';');
            }
            else {
                $scope.booking.ExternalNames = []; //Reset External Name List
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
        }
      )
    }

    $scope.CheckEditBooking = function () {
        //Reset Any Error Messages
        SetModalErrorMessage('');

        //Validate Start Date
        if ($scope.booking.date === "") {
            SetModalErrorMessage('Invalid Date.');
            return;
        }

        //Validate Number of External Names is not graether than attendees
        ValidateNoAttendees($scope.booking.numberOfAttendees, $scope.booking.ExternalNames.length);
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
            $scope.newBooking.StartDate = FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.startTime, 'MM-DD-YYYY-HHmm', true);
            $scope.newBooking.EndDate = FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.endTime, 'MM-DD-YYYY-HHmm', true);

            if ($scope.booking.ExternalNames.length > 0) {
                $scope.newBooking.ExternalNames = $scope.booking.ExternalNames.join(';');
            }

            //Validate if Date/Time/Attendees has changed
            if (($scope.booking.numberOfAttendees === $scope.existingBooking.numberOfAttendees) && ($scope.booking.date === $scope.existingBooking.date) &&
                ($scope.booking.startTime === $scope.existingBooking.startTime) && ($scope.booking.endTime === $scope.existingBooking.endTime)) {
                SaveEditBooking($scope.bookingId, $scope.newBooking);
            }
            else {
                $scope.availableRooms = Available.query({
                    location: $scope.editBooking.location.name,
                    startDate: FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.startTime, 'MM-DD-YYYY-HHmm', true),
                    endDate: FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.endTime, 'MM-DD-YYYY-HHmm', true),
                    smartRoom: false,
                    numberOfAttendees: $scope.booking.numberOfAttendees,
                    existingBookingId: $scope.bookingId
                },
                function (success) {
                    if ($scope.availableRooms.length === 0) {
                        EnableAcceptBookingButton();
                        SetModalErrorMessage('No Rooms Avaliable using the below Date/Time/Attendees.');
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
                location.reload();
            },
            function (error) {
                EnableDeleteBookingButton();
                alert('Unable to Delete Booking. Please Try Again or Contact ITSD. ' + error.message);
            });
        } catch (e) {
            EnableDeleteBookingButton();
        }
    }

    $scope.SearchBooking = function () {
        SetAdminErrorMessage('');

        //Validate Full Name
        if ($('#fullNameTextBox').typeahead('val').trim() <= 0) {
            SetAdminErrorMessage("Invalid Full Name");
        }

        $scope.bookings = GetBookings.query({
            owner: $('#fullNameTextBox').typeahead('val'),
            start: FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + "12:00", 'MM-DD-YYYY', true)
        },
        function (success) {
            if (success.length === 0) {
                SetAdminErrorMessage('No Bookings for ' + $('#fullNameTextBox').typeahead('val') + ' on the ' + $scope.bookingFilter.startDate);
            }
        });
    }

    $scope.newBooking = {
        Room: { RoomName: '' },
        Subject: '',
        NumberOfAttendees: 0,
        RoomID: 0,
        ExternalNames: null,
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
        externalNames: []
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