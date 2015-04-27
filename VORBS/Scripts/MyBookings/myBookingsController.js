myBookings.controller('MyBookingsController', ['$scope', '$http', '$resource', MyBookingsController]);

function MyBookingsController($scope, $http, $resource) {

    CreateServices($resource);

    $scope.bookings = Booking.query({});
}


function CreateServices($resource) {
    Booking = $resource('/api/bookings/' + new moment().add('week', -2).format("MM-DD-YYYY") + '/' + new moment().add('week', 2).format("MM-DD-YYYY") + '/' + 'Reece', {
    }, {
        query: { method: 'GET', isArray: true }
    });

    Booking.prototype = {
        startDateFormatted: function () { debugger; return moment(this.startDate).format("DD/MM/YYYY - hh:mm"); },
        endDateFormatted: function () { return moment(this.endDate).format("DD/MM/YYYY - hh:mm"); }
    };
}