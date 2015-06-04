users.controller('UsersController', ['$scope', '$http', '$resource', UsersController]);

function UsersController($scope, $http, $resource) {
    CreateServices($resource)
}

function CreateServices($resource) {
    Users = $resource('/api/admin/:userId', { userId: 'userId' },
    {
        getAll: { method: 'GET', isArray: true },
        getUserById: {method: 'GET', isArray: true }
    });

    GetBookings.prototype = {
        DateFormatted: function () { return moment(this.startDate).format("DD/MM/YYYY"); },
        startTimeFormatted: function () { return moment(this.startDate).format("H:mm"); },
        endTimeFormatted: function () { return moment(this.endDate).format("H:mm"); }
    };
}