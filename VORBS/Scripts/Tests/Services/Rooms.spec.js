describe('Rooms Service', function () {

    var RoomsService;
    var $httpBackend;

    beforeEach(angular.mock.module('vorbs.services'));

    beforeEach(inject(function (_RoomsService_, _$httpBackend_) {
        $httpBackend = _$httpBackend_;
        RoomsService = _RoomsService_;
    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
    });

    it('should be defined', function () {
        expect(RoomsService).toBeDefined();
    });

    describe('query method', function () {

        it('should exist', function () {
            expect(RoomsService.query).not.toEqual(null);
            expect(RoomsService.query).toBeDefined();
        })

        it('should return an array of all rooms', function () {

            var rooms = roomMockData.getAllRooms();

            $httpBackend.expectGET('/api/room')
                .respond(200, rooms);

            var result = RoomsService.query();

            $httpBackend.flush();

            expect(result instanceof Array).toBeTruthy();
            expect(result.length).toEqual(2);
        });
    });


    describe('getByID method', function () {

        it('should exist', function () {
            expect(RoomsService.getByID).not.toEqual(null);
            expect(RoomsService.getByID).toBeDefined();
        })

        it('should return a room object when passed an ID', function () {

            var room = roomMockData.getSingleMockRoom;

            $httpBackend.expectGET('/api/room/' + room.id)
                .respond(200, room);

            var result = RoomsService.getByID({
                id: room.id
            });

            $httpBackend.flush();

            expect(result).not.toEqual(null);
            expect(result.roomName).toEqual(room.roomName);
        });
    });


    describe('getByStatus method', function () {

        it('should exist', function () {
            expect(RoomsService.getByStatus).not.toEqual(null);
            expect(RoomsService.getByStatus).toBeDefined();
        })

        it('should return a set of rooms based on the parameters provided', function () {

            var rooms = roomMockData.getAllRooms(),
                locationName = "LocationOne",
                status = 0;

            $httpBackend.expectGET('/api/room/' + locationName + '/' + status)
                .respond(200, rooms);

            var result = RoomsService.getByStatus({
                locationName: locationName,
                status: status
            });

            $httpBackend.flush();

            expect(result).not.toEqual(null);
            expect(result instanceof Array).toBeTruthy();
            expect(result.length).toEqual(rooms.length);
        });
    });

    describe('getByName method', function () {

        it('should exist', function () {
            expect(RoomsService.getByName).not.toEqual(null);
            expect(RoomsService.getByName).toBeDefined();
        })

        it('should return a room object based on the location id and room name provided', function () {

            var room = roomMockData.getSingleMockRoom,
                locationId = room.location.id,
                roomName = room.roomName;

            $httpBackend.expectGET('/api/room/' + locationId + '/' + roomName)
                .respond(200, room);

            var result = RoomsService.getByName({
                locationId: locationId,
                roomName: roomName
            });

            $httpBackend.flush();

            expect(result).not.toEqual(null);
            expect(result.roomName).toEqual(room.roomName);
            expect(result.location.id).toEqual(room.location.id);
        });
    });


    describe('create method', function () {

        it('should exist', function () {
            expect(RoomsService.create).not.toEqual(null);
            expect(RoomsService.create).toBeDefined();
        });

        it('should create a room object', function () {

            var room = roomMockData.getSingleMockRoom;

            $httpBackend.expectPOST('/api/room', room)
                .respond(200);

            RoomsService.create({}, room);

            expect($httpBackend.flush).not.toThrow();

        });
    });


    describe('update method', function () {

        it('should exist', function () {
            expect(RoomsService.update).not.toEqual(null);
            expect(RoomsService.update).toBeDefined();
        });

        it('should update an existing room object', function () {

            var room = roomMockData.getSingleMockRoom;

            $httpBackend.expectPUT('/api/room/' + room.id, room)
                .respond(200);

            RoomsService.update({
                id: room.id
            }, room);

            expect($httpBackend.flush).not.toThrow();
        });
    });



    describe('updateStatus method', function () {

        it('should exist', function () {
            expect(RoomsService.updateStatus).not.toEqual(null);
            expect(RoomsService.updateStatus).toBeDefined();
        });

        it('should update an existing rooms status', function () {

            var room = roomMockData.getSingleMockRoom,
                roomStatus = false;

            $httpBackend.expectPATCH('/api/room/' + room.id + '/' + roomStatus)
                .respond(200);

            RoomsService.updateStatus({
                id: room.id,
                status: roomStatus
            });

            expect($httpBackend.flush).not.toThrow();

        });
    });

});