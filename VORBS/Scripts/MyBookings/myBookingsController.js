myBookings.controller('MyBookingsController', ['$scope', '$http', '$resource', MyBookingsController]);

function MyBookingsController($scope, $http, $resource) {

    CreateServices($resource);

    $scope.bookings = GetBookings.query({
        startDate: new moment().utc().format("MM-DD-YYYY-HHmm"),
        //person: '7220451' //TODO: Change To get UserName
    });

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
        ResetExternalNamesUI();
        SetEditActiveTab('editBooking');

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
    });

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

        //Validate Subject
        //var Subject = ValidateSubject($scope.newBooking.Subject);

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

        //Validate if Date/Time/Attendees has changed
        if (($scope.booking.numberOfAttendees === $scope.existingBooking.numberOfAttendees) && ($scope.booking.date === $scope.existingBooking.date) &&
            ($scope.booking.startTime === $scope.existingBooking.startTime) && ($scope.booking.endTime === $scope.existingBooking.endTime)) {
            SaveEditBooking($scope.bookingId, $scope.newBooking);
        }
        else {
            $scope.availableRooms = Available.query({
                location: $scope.editBooking.location.name,
                startDate: FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.startTime, 'MM-DD-YYYY-HHmm'),
                endDate: FormatDateTimeForURL($scope.booking.date + ' ' + $scope.booking.endTime, 'MM-DD-YYYY-HHmm'),
                smartRoom: false,
                numberOfAttendees: $scope.booking.numberOfAttendees,
                existingBookingId: $scope.bookingId
            },
            function (success) {
                if ($scope.availableRooms.length === 0) {
                    SetModalErrorMessage('No Rooms Avaliable using the below Date/Time/Attendees.');
                }
                else if ($scope.availableRooms[0].roomName.replace('_', '.') === $scope.newBooking.Room.RoomName) {
                    SaveEditBooking($scope.bookingId, $scope.newBooking);
                }
                else {
                    SetEditActiveTab('confirmEditBooking');
                    $scope.currentRoom = $scope.availableRooms[0];
                }
            },
            function (error) {
                //TODO:Log Error
                alert('Unable to Edit. Please Try Again or Contact ITSD. ' + error.message);
            });
        }
    }

    $scope.EditBooking = function () {
        $scope.newBooking.RoomID = $scope.currentRoom.id;
        SaveEditBooking($scope.bookingId, $scope.newBooking);
    }

    $scope.DeleteBooking = function () {
        Booking.remove(
            {
                bookingId: $scope.bookingId
            },
            function (success) {
                location.reload();
            },
            function (error) {
                //TODO:Log Error
                alert('Unable to Delete Booking. Please Try Again or Contact ITSD. ' + error.message);
            }
        );
    }

    $('.datepicker').datepicker({
        startDate: '-0m',
        format: 'dd-mm-yyyy',
        autoClose: true,
        todayHighlight: true,
        weekStart: 1,
        daysOfWeekDisabled: [0, 6]
    });

    $('.timepicker').timepicker({
        showInputs: false,
        minuteStep: 30,
        showMeridian: false
    });

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
}


function CreateServices($resource) {
    Available = $resource('/api/availability/:location/:startDate/:endDate/:numberOfAttendees/:smartRoom/:existingBookingId', {
        location: 'location', startDate: 'startDate', endDate: 'endDate', numberOfAttendees: 'numberOfAttendees', smartRoom: 'smartRoom', existingBookingId: 'existingBookingId'
    },
    {
        query: { method: 'GET', isArray: true }
    });

    Available.prototype = {
        roomNameFormatted: function () { return this.roomName.replace('_', '.'); }
    };

    GetBookings = $resource('/api/bookings/:startDate/:person', { startDate: 'startDate', person: 'person' },
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
}