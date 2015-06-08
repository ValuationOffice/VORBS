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

    $('#editModal').on('show.bs.modal', function () {
        //Reset Any Error Messages
        SetModalErrorMessage('');

        $scope.editBooking = Booking.query({
            bookingId: $scope.bookingId
        },
        function (sucess) {
            $scope.newBooking.Room.RoomName = $scope.editBooking.room.roomName;
            $scope.newBooking.Subject = $scope.editBooking.subject;
            $scope.newBooking.NumberOfAttendees = $scope.editBooking.numberOfAttendees;
            $scope.newBooking.FlipChart = $scope.editBooking.flipchart;
            $scope.newBooking.Projector = $scope.editBooking.projector;

            if ($scope.editBooking.externalNames !== null) {
                $scope.booking.ExternalNames = $scope.editBooking.externalNames.split(';');
            }
            else {
                $scope.booking.ExternalNames = []; //Reset External Name List
            }

            $scope.booking.startTime = FormatTimeDate($scope.editBooking.startDate, false);
            $scope.booking.endTime = FormatTimeDate($scope.editBooking.endDate, false);
            $scope.booking.date = FormatTimeDate($scope.editBooking.startDate, true);
        }
      )
    });

    $('#editModal').on('show.bs.modal', function () {
        ResetExternalNamesUI();
    });

    $('#fullNameTextBox').typeahead({
        hint: true,
        highlight: true,
        minLength: 1
    },
    {
        name: 'owners',
        source: SubstringMatcher($scope.owners)
    });

    $scope.EditBooking = function () {
        //Reset Any Error Messages
        SetModalErrorMessage('');

        //Validate Start Date
        if ($scope.booking.date === "") {
            SetModalErrorMessage('Invalid Date.');
            return;
        }

        //Validate Number of External Names is not graether than attendees
        ValidateNoAttendees($scope.newBooking.NumberOfAttendees, $scope.booking.ExternalNames.length);

        //Validate Subject
        var Subject = ValidateSubject($scope.newBooking.Subject);

        //Validate Times
        var timeValidation = ValidateStartEndTime($scope.booking.startTime, $scope.booking.endTime);
        if (timeValidation !== "") {
            SetModalErrorMessage(timeValidation);
            return;
        }

        //Create Date String
        $scope.newBooking.StartDate = FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.startTime, 'MM-DD-YYYY-HHmm');
        $scope.newBooking.EndDate = FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.endTime, 'MM-DD-YYYY-HHmm');

        if ($scope.booking.ExternalNames.length > 0) {
            $scope.newBooking.ExternalNames = $scope.booking.ExternalNames.join(';');
        }

        $.ajax({
            type: "POST",
            data: JSON.stringify($scope.newBooking),
            url: "api/bookings/" + $scope.bookingId,
            contentType: "application/json",
            success: function (data, status) {
                alert('Booking Edited. Confirmation Email Have Been Sent.');
                window.location.href = "/Administration"; //Redirect to my bookings
            },
            error: function (error) {
                if (error.status = 409) {
                    SetModalErrorMessage(error.responseJSON);
                }
                else {
                    alert('Unable to edit Meeting Room. Please Contact ITSD. ' + error.responseJSON);
                }
            }
        });
    }

    $scope.DeleteBooking = function () {
        Booking.remove(
            {
                bookingId: $scope.bookingId
            },
            function (success) {
                //TODO: Change ?
                location.reload();
            },
            function (error) {
                //TODO:Log Error
                alert('Unable to Delete Booking. Please Try Again or Contact ITSD.');
                location.reload();
            }
        );
    }

    $scope.SearchBooking = function () {
        SetAdminErrorMessage('');

        //Validate Full Name
        if ($('#fullNameTextBox').typeahead('val').trim() <= 0) {
            SetAdminErrorMessage("Invalid Full Name");
        }

        $scope.bookings = GetBooking.query({
            owner: $('#fullNameTextBox').typeahead('val'),
            start: FormatDateTimeForURL($scope.bookingFilter.startDate + ' ' + "12:00", 'MM-DD-YYYY')
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
        externalNames: []
    }

    $scope.bookingFilter = {
        fullName: '',
        startDate: new moment().utc().format('DD-MM-YYYY')
    }
}

function CreateBookingServices($resource) {
    Booking = $resource('/api/bookings/:bookingId', { bookingId: 'bookingId', adminId: 'adminId' },
    {
        query: { method: 'GET' },
        remove: { method: 'DELETE' }
    });

    GetBooking = $resource('api/bookings/:owner/:start', { owner: 'owner', start: 'start' },
    {
        query: {method: 'GET', isArray: true }
    });

    GetBooking.prototype = {
        DateFormatted: function () { return moment(this.startDate).format("DD/MM/YYYY"); },
        startTimeFormatted: function () { return moment(this.startDate).format("H:mm"); },
        endTimeFormatted: function () { return moment(this.endDate).format("H:mm"); }
    };

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