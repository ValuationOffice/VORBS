administration.controller('MyBookingsController', ['$scope', '$http', '$resource', MyBookingsController]);

function MyBookingsController($scope, $http, $resource) {

    CreateBookingServices($resource);

    $scope.bookings = GetAllBookings.query({
        startDate: new moment().utc().format("MM-DD-YYYY-HHmm")
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

    $('.datepicker').datepicker({
        startDate: '-0m',
        format: 'dd-mm-yyyy',
        autoClose: true,
        todayBtn: true,
        todayHighlight: true,
        weekStart: 1
    });

    $('.timepicker').timepicker({
        showInputs: false,
        minuteStep: 30,
        showMeridian: false
    });

    $scope.EditBooking = function () {
        //Validate Number of External Names is not graether than attendees
        ValidateNoAttendees($scope.newBooking.NumberOfAttendees, $scope.booking.ExternalNames.length);

        //Validate Subject
        var Subject = ValidateSubject($scope.newBooking.Subject);

        //Validate Times

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
                window.location.href = "/Bookings"; //Redirect to my bookings
            },
            error: function (error) {
                if (error.status = 409) {
                    SetModalErrorMessage(error.responseJSON.message);
                }
                else {
                    alert('Unable to edit Meeting Room. Please Contact ITSD. ' + error.responseJSON.message);
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
}

function CreateBookingServices($resource) {
    GetAllBookings = $resource('/api/bookings/:startDate', { startDate: 'startDate' },
   {
       query: { method: 'GET', isArray: true }
   });

    GetAllBookings.prototype = {
        DateFormatted: function () { return moment(this.startDate).format("DD/MM/YYYY"); },
        startTimeFormatted: function () { return moment(this.startDate).format("H:mm"); },
        endTimeFormatted: function () { return moment(this.endDate).format("H:mm"); }
    };

    Booking = $resource('/api/bookings/:bookingId', { bookingId: 'bookingId', adminId: 'adminId' },
    {
        query: { method: 'GET' },
        remove: { method: 'DELETE' }
    });

}