import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:flutter/foundation.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/app_exception.dart';
import '../models/content_schedule_model.dart';
import '../models/create_schedule_request.dart';

import '../models/update_schedule_request.dart';

part 'calendar_repository.g.dart';

List<ContentScheduleModel> _parseContentScheduleList(List<dynamic> items) {
  return items.map((e) => ContentScheduleModel.fromJson(e)).toList();
}

class CalendarRepository {
  final Dio _dio;

  CalendarRepository(this._dio);

  Future<List<ContentScheduleModel>> getSchedules({int page = 1, int pageSize = 100}) async {
    try {
      final queryParams = {
        'page': page,
        'pageSize': pageSize,
      };
      final response = await _dio.get('/content-schedules', queryParameters: queryParams);
      final data = response.data['data'];
      final items = data != null ? data['data'] as List? : null;
      if (items == null || items.isEmpty) return [];
      return await compute(_parseContentScheduleList, items);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<List<ContentScheduleModel>> getUpcomingSchedules({int limit = 100}) async {
    try {
      final queryParams = {
        'limit': limit,
      };
      final response = await _dio.get('/content-schedules/upcoming', queryParameters: queryParams);
      final items = response.data['data'] as List;
      if (items.isEmpty) return [];
      return await compute(_parseContentScheduleList, items);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ContentScheduleModel> getScheduleById(String id) async {
    try {
      final response = await _dio.get('/content-schedules/$id');
      return ContentScheduleModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ContentScheduleModel> createSchedule(CreateScheduleRequest request) async {
    try {
      final response = await _dio.post('/content-schedules', data: request.toJson());
      return ContentScheduleModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<ContentScheduleModel> updateSchedule(String id, UpdateScheduleRequest request) async {
    try {
      final response = await _dio.put('/content-schedules/$id', data: request.toJson());
      return ContentScheduleModel.fromJson(response.data['data']);
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }

  Future<void> deleteSchedule(String id) async {
    try {
      await _dio.delete('/content-schedules/$id');
    } catch (e) {
      throw ExceptionHandler.handle(e);
    }
  }
}

@riverpod
CalendarRepository calendarRepository(CalendarRepositoryRef ref) {
  return CalendarRepository(ref.read(dioProvider));
}
