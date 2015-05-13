myBookings.controller('MyBookingsController', ['$scope', '$http', '$resource', MyBookingsController]);

function MyBookingsController($scope, $http, $resource) {

    CreateServices($resource);

    $scope.bookings = Booking.query({});
}


function CreateServices($resource) {
    //Only retreives bookings in the next 3 months. TODO: Confirm ?
    Booking = $resource('/api/bookings/' + new moment().utc().format("MM-DD-YYYY-HHmm"), {
    }, {
        query: { method: 'GET', isArray: true }
    });

    Booking.prototype = {
        startDateFormatted: function () { return moment(this.startDate).format("DD/MM/YYYY - hh:mm A"); },
        endDateFormatted: function () { return moment(this.endDate).format("DD/MM/YYYY - hh:mm A"); }
    };
}