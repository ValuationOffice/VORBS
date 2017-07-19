(function () {

    angular.module('vorbs.services')
        .factory('Bookings', BookingsService);

    BookingsService.$inject = ['$resource'];

    function BookingsService($resource) {

        return $resource('/api/bookings', {}, {
            search: {
                method: 'GET',
                isArray: true,
                url: '/api/bookings/search'
            },
            getByID: {
                method: 'GET',
                params: {
                    bookingId: '@bookingId'
                },
                url: '/api/bookings/:bookingId'
            },
            getByPeriod: {
                method: 'GET',
                isArray: true,
                params: {
                    startDate: '@startDate',
                    period: '@period'
                },
                url: '/api/bookings/:startDate/:period'
            },
            create: {
                method: 'POST'                
            },
            update: {
                method: 'POST',
                params: {
                    existingId: '@existingId',
                    recurrence: '@recurrence'
                },
                url: '/api/bookings/:existingId/:recurrence'
            },
            remove: {
                method: 'DELETE',
                params: {
                    bookingId: '@bookingId',
                    recurrence: '@recurrence'
                },
                url: '/api/bookings/:bookingId/:recurrence'
            }
        });
    }

})();