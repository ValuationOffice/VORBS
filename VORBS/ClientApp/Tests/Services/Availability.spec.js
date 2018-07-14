describe('Availability Service', function () {

    var AvailabilityService;
    var locationName = 'TestLocation';
    var $httpBackend;

    beforeEach(angular.mock.module('vorbs.services'));

    beforeEach(inject(function (_AvailabilityService_, _$httpBackend_) {
        $httpBackend = _$httpBackend_;
        AvailabilityService = _AvailabilityService_;
    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
    });

    it('should be defined', function () {
        expect(AvailabilityService).toBeDefined();
    });

    describe('get method', function () {
        it('should exist', function () {
            expect(AvailabilityService.get).not.toEqual(null);
            expect(AvailabilityService.get).toBeDefined();
        });

        it('should return an array of room objects', function () {

            var availableRooms = availabilityMockData.getAllRoomsForLocation();
            var queryDate = "08-07-2017-0900";
            var smartRoom = false;

            $httpBackend.expectGET('/api/availability/' + locationName + '/' + queryDate +'/' + smartRoom)
                .respond(200, availableRooms);

            var result = AvailabilityService.get({
                location: locationName,
                start: queryDate,
                smartRoom: smartRoom
            });

            $httpBackend.flush();

            expect(result).not.toEqual(null);
            expect(result instanceof Array).toBeTruthy();
            expect(result.length).toEqual(2);
        });

        it('should return a filtered array of room objects', function () {

            var availableRooms = availabilityMockData.getAvailableRooms();
            var queryStartDate = "09-07-2017-0900";
            var queryEndDate = "09-07-2017-1030";
            var numberOfPeople = 5;
            var smartRoom = false;

            $httpBackend.expectGET('/api/availability/' + locationName + '/' + queryStartDate + '/' + smartRoom + '/' + queryEndDate + '/' + numberOfPeople)
                .respond(200, availableRooms);

            var result = AvailabilityService.get({
                location: locationName,
                start: queryStartDate,
                end: queryEndDate,
                numberOfPeople: numberOfPeople,
                smartRoom: smartRoom
            });

            $httpBackend.flush();

            expect(result).not.toEqual(null);
            expect(result instanceof Array).toBeTruthy();
            expect(result.length).toEqual(1);
        });

    });

});