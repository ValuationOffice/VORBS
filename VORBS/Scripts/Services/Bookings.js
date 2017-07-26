(function () {

    angular.module('vorbs.services')
        .factory('BookingsService', BookingsService);

    BookingsService.$inject = ['$resource', 'dateFilter'];

    function BookingsService($resource, dateFilter) {

        function formatDate(dateTime) {
            return dateFilter(dateTime, 'dd/MM/yyyy');
        }

        function formatTime(dateTime) {
            return dateFilter(dateTime, 'H:mm');
        }

        function formatBooking(booking) {

            booking.start = {
                date: formatDate(booking.startDate),
                time: formatTime(booking.startDate)
            };

            booking.end = {
                date: formatDate(booking.endDate),
                time: formatTime(booking.endDate)
            };

            return booking;
        }


        return $resource('/api/bookings', {}, {
            search: {
                method: 'GET',
                isArray: true,
                interceptor: {
                    response: function (resp) {

                        var data = resp.data;

                        if (data) {
                            angular.forEach(data, function (value) {
                                value = formatBooking(value);
                            });
                        }

                        return data;
                    }
                },
                url: '/api/bookings/search'
            },
            getByID: {
                method: 'GET',
                params: {
                    bookingId: '@bookingId'
                },
                interceptor: {
                    response: function (resp) {
                        var data = resp.data;
                        if (data) data = formatBooking(data);
                        return data;
                    }
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
                interceptor: {
                    response: function (resp) {

                        var data = resp.data;

                        if (data) {
                            angular.forEach(data, function (value) {
                                value = formatBooking(value);
                            });
                        }

                        return data;
                    }
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