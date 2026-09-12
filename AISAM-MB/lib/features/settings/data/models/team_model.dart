class TeamSummaryModel {
  final String id;
  final String name;
  final String? description;
  final String status;
  final int memberCount;
  final int brandCount;
  final DateTime createdAt;
  final bool hasManager;

  const TeamSummaryModel({
    required this.id,
    required this.name,
    this.description,
    required this.status,
    required this.memberCount,
    required this.brandCount,
    required this.createdAt,
    this.hasManager = false,
  });

  factory TeamSummaryModel.fromJson(Map<String, dynamic> json) {
    return TeamSummaryModel(
      id: json['id'] as String? ?? '',
      name: json['name'] as String? ?? '',
      description: json['description'] as String?,
      status: json['status'] as String? ?? 'Active',
      memberCount: (json['memberCount'] as num?)?.toInt() ?? 0,
      brandCount: (json['brandCount'] as num?)?.toInt() ?? 0,
      createdAt: json['createdAt'] != null
          ? DateTime.tryParse(json['createdAt'] as String) ?? DateTime.now()
          : DateTime.now(),
      hasManager: json['hasManager'] as bool? ?? false,
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'name': name,
        'description': description,
        'status': status,
        'memberCount': memberCount,
        'brandCount': brandCount,
        'createdAt': createdAt.toIso8601String(),
        'hasManager': hasManager,
      };
}

class TeamMemberItemModel {
  final String userId;
  final String name;
  final String email;
  final String role;
  final DateTime joinedAt;
  final bool isActive;

  const TeamMemberItemModel({
    required this.userId,
    required this.name,
    required this.email,
    required this.role,
    required this.joinedAt,
    required this.isActive,
  });

  factory TeamMemberItemModel.fromJson(Map<String, dynamic> json) {
    return TeamMemberItemModel(
      userId: json['userId'] as String? ?? '',
      name: json['name'] as String? ?? '',
      email: json['email'] as String? ?? '',
      role: json['role'] as String? ?? 'Member',
      joinedAt: json['joinedAt'] != null
          ? DateTime.tryParse(json['joinedAt'] as String) ?? DateTime.now()
          : DateTime.now(),
      isActive: json['isActive'] as bool? ?? true,
    );
  }

  Map<String, dynamic> toJson() => {
        'userId': userId,
        'name': name,
        'email': email,
        'role': role,
        'joinedAt': joinedAt.toIso8601String(),
        'isActive': isActive,
      };
}

class TeamBrandItemModel {
  final String brandId;
  final String brandName;
  final bool isActive;
  final DateTime assignedAt;

  const TeamBrandItemModel({
    required this.brandId,
    required this.brandName,
    required this.isActive,
    required this.assignedAt,
  });

  factory TeamBrandItemModel.fromJson(Map<String, dynamic> json) {
    return TeamBrandItemModel(
      brandId: json['brandId'] as String? ?? '',
      brandName: json['brandName'] as String? ?? '',
      isActive: json['isActive'] as bool? ?? true,
      assignedAt: json['assignedAt'] != null
          ? DateTime.tryParse(json['assignedAt'] as String) ?? DateTime.now()
          : DateTime.now(),
    );
  }

  Map<String, dynamic> toJson() => {
        'brandId': brandId,
        'brandName': brandName,
        'isActive': isActive,
        'assignedAt': assignedAt.toIso8601String(),
      };
}

class TeamDetailModel {
  final String id;
  final String name;
  final String? description;
  final String status;
  final DateTime createdAt;
  final DateTime? updatedAt;
  final List<TeamMemberItemModel> members;
  final List<TeamBrandItemModel> brands;

  const TeamDetailModel({
    required this.id,
    required this.name,
    this.description,
    required this.status,
    required this.createdAt,
    this.updatedAt,
    required this.members,
    required this.brands,
  });

  factory TeamDetailModel.fromJson(Map<String, dynamic> json) {
    return TeamDetailModel(
      id: json['id'] as String? ?? '',
      name: json['name'] as String? ?? '',
      description: json['description'] as String?,
      status: json['status'] as String? ?? 'Active',
      createdAt: json['createdAt'] != null
          ? DateTime.tryParse(json['createdAt'] as String) ?? DateTime.now()
          : DateTime.now(),
      updatedAt: json['updatedAt'] != null
          ? DateTime.tryParse(json['updatedAt'] as String)
          : null,
      members: (json['members'] as List<dynamic>?)
              ?.map((e) => TeamMemberItemModel.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      brands: (json['brands'] as List<dynamic>?)
              ?.map((e) => TeamBrandItemModel.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'name': name,
        'description': description,
        'status': status,
        'createdAt': createdAt.toIso8601String(),
        'updatedAt': updatedAt?.toIso8601String(),
        'members': members.map((e) => e.toJson()).toList(),
        'brands': brands.map((e) => e.toJson()).toList(),
      };
}

class CreditWalletModel {
  final int balance;
  final String workspaceId;

  const CreditWalletModel({
    required this.balance,
    required this.workspaceId,
  });

  factory CreditWalletModel.fromJson(Map<String, dynamic> json) {
    return CreditWalletModel(
      balance: (json['balance'] as num?)?.toInt() ?? 0,
      workspaceId: json['workspaceId'] as String? ?? '',
    );
  }

  Map<String, dynamic> toJson() => {
        'balance': balance,
        'workspaceId': workspaceId,
      };
}
