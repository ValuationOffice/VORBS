describe('Bookings Service', function () {

    var BookingsService;
    var $httpBackend;

    beforeEach(angular.mock.module('vorbs.services'));

    beforeEach(inject(function (_Bookings_, _$httpBackend_) {
        $httpBackend = _$httpBackend_;
        BookingsService = _Bookings_;
    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
    });

    it('should be defined', function () {
        expect(BookingsService).toBeDefined();
    });

    describe('search method', function () {

        it('should exist', function () {
            expect(BookingsService.search).not.toEqual(null);
            expect(BookingsService.search).toBeDefined();
        })

        it('should return bookings based on the parameters provided', function () {

            var bookings = [];
            var mockBooking = bookingMockData.getSingleMockBooking;
            bookings.push(mockBooking);

            $httpBackend.expectGET('/api/bookings/search?locationId=' + mockBooking.location.id + '&room=' + mockBooking.room.roomName + '&smartRoom=false&startDate=07-20-2017')
                .respond(200, bookings);

            var result = BookingsService.search({
                locationId: mockBooking.location.id,
                room: mockBooking.room.roomName,
                smartRoom: false,
                startDate: '07-20-2017'
            });

            $httpBackend.flush();

            expect(result instanceof Array).toBeTruthy();
            expect(result.length).toEqual(1);
        });
    });

    describe('getByID method', function () {

        it('should exist', function () {
            expect(BookingsService.getByID).not.toEqual(null);
            expect(BookingsService.getByID).toBeDefined();
        })

        it('should return a booking object when passed an ID', function () {

            var booking = bookingMockData.getSingleMockBooking;

            $httpBackend.expectGET('/api/bookings/' + booking.id)
                .respond(200, booking);

            var result = BookingsService.getByID({
                bookingId: booking.id
            });

            $httpBackend.flush();

            expect(result).not.toEqual(null);
            expect(result.owner).toEqual(booking.owner);       
        });
    });

    describe('getByPeriod method', function () {

        it('should exist', function () {
            expect(BookingsService.getByPeriod).not.toEqual(null);
            expect(BookingsService.getByPeriod).toBeDefined();
        })

        it('should return a set of bookings', function () {

            var bookings = bookingMockData.getMockBookings();

            $httpBackend.expectGET('/api/bookings/07-18-2017-1455/7')
                .respond(200, bookings);

            var result = BookingsService.getByPeriod({
                startDate: '07-18-2017-1455',
                period: 7
            });

            $httpBackend.flush();

            expect(result).not.toEqual(null);
            expect(result instanceof Array).toBeTruthy();
            expect(result.length).toEqual(bookings.length);
        });
    });

    describe('create method', function () {

        it('should exist', function () {
            expect(BookingsService.create).not.toEqual(null);
            expect(BookingsService.create).toBeDefined();
        });

        it('should create a booking object', function () {

            var booking = bookingMockData.getSingleMockBooking;

            $httpBackend.expectPOST('/api/bookings', booking)
                .respond(200);

            BookingsService.create({}, booking);

            expect($httpBackend.flush).not.toThrow();

        });
    });

    describe('update method', function () {

        it('should exist', function () {
            expect(BookingsService.update).not.toEqual(null);
            expect(BookingsService.update).toBeDefined();
        });

        it('should update an existing booking object', function () {

            var booking = bookingMockData.getSingleMockBooking;
            var recurrence = false;

            $httpBackend.expectPOST('/api/bookings/' + booking.id + '/' + recurrence, booking)
                .respond(200);

            BookingsService.update({
                existingId: booking.id,
                recurrence: recurrence
            }, booking);

            expect($httpBackend.flush).not.toThrow();

        });
    });

    describe('remove method', function () {

        it('should exist', function () {
            expect(BookingsService.remove).not.toEqual(null);
            expect(BookingsService.remove).toBeDefined();
        });

        it('should remove an existing booking object', function () {

            var booking = bookingMockData.getSingleMockBooking;
            var recurrence = false;

            $httpBackend.expectDELETE('/api/bookings/' + booking.id + '/' + recurrence)
                .respond(200);

            BookingsService.remove({
                bookingId: booking.id,
                recurrence: recurrence
            });

            expect($httpBackend.flush).not.toThrow();

        });
    });

});