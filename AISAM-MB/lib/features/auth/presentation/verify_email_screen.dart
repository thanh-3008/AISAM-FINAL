import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';

class VerifyEmailScreen extends StatefulWidget {
  final String? email;
  const VerifyEmailScreen({super.key, this.email});

  @override
  State<VerifyEmailScreen> createState() => _VerifyEmailScreenState();
}

class _VerifyEmailScreenState extends State<VerifyEmailScreen> with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _floatingAnimation;
  bool _isResent = false;

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      vsync: this,
      duration: const Duration(seconds: 3),
    )..repeat(reverse: true);
    
    _floatingAnimation = Tween<double>(begin: 0, end: -10).animate(
      CurvedAnimation(parent: _animationController, curve: Curves.easeInOut),
    );
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  void _onResendEmail() {
    if (_isResent) return;
    setState(() => _isResent = true);
    Future.delayed(const Duration(seconds: 3), () {
      if (mounted) {
        setState(() => _isResent = false);
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    final displayEmail = widget.email ?? 'alex.design@aisam.io';

    return Scaffold(
      backgroundColor: const Color(0xFFF7F9FB),
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios_new, color: Color(0xFF191C1E)),
          onPressed: () => context.pop(),
        ),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20.0),
          child: Column(
            children: [
              const SizedBox(height: 32),
              // Visual Section
              SizedBox(
                height: 200,
                width: double.infinity,
                child: Stack(
                  alignment: Alignment.center,
                  children: [
                    // Ambient Background
                    Container(
                      width: 200,
                      height: 200,
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        color: const Color(0xFF003EC7).withOpacity(0.05),
                      ),
                    ),
                    // Floating Card
                    AnimatedBuilder(
                      animation: _floatingAnimation,
                      builder: (context, child) {
                        return Transform.translate(
                          offset: Offset(0, _floatingAnimation.value),
                          child: child,
                        );
                      },
                      child: Stack(
                        clipBehavior: Clip.none,
                        children: [
                          Container(
                            width: 128,
                            height: 128,
                            decoration: BoxDecoration(
                              color: Colors.white,
                              borderRadius: BorderRadius.circular(32),
                              boxShadow: [
                                BoxShadow(
                                  color: Colors.black.withOpacity(0.05),
                                  blurRadius: 20,
                                  offset: const Offset(0, 10),
                                ),
                              ],
                            ),
                            alignment: Alignment.center,
                            child: Container(
                              width: 80,
                              height: 80,
                              decoration: const BoxDecoration(
                                color: Color(0xFFD2E0FE),
                                shape: BoxShape.circle,
                              ),
                              alignment: Alignment.center,
                              child: Image.network(
                                'https://lh3.googleusercontent.com/aida-public/AB6AXuDxyfQnGF-HaWQL5j0xVsAQNFw0ohb0utlOJJ6AbIF1bZM44VJlZgx0kfXSLEPA-YYjx0n0l6e7I4aRa92ccF6kxzkgsbH_zC5fYk3wXpGE0F2n0q38YlIqyx2I5OqnnP-gmE1r4Gd7zWPspErQbyG2a5NbyC0ij0JWkA6biE1HKeKfdpMcEqRj_8i3so5ljLjtu2fCqiTKXZTlkia09mE-tHiC5Yxh6M7A6DtQYMOVDoh4MJUgHigp',
                                width: 48,
                                height: 48,
                                fit: BoxFit.contain,
                                errorBuilder: (_, __, ___) => const Icon(Icons.email, size: 40, color: Color(0xFF003EC7)),
                              ),
                            ),
                          ),
                          Positioned(
                            top: -16,
                            right: -16,
                            child: Transform.rotate(
                              angle: 0.2,
                              child: Container(
                                width: 48,
                                height: 48,
                                decoration: BoxDecoration(
                                  color: const Color(0xFF0052FF),
                                  borderRadius: BorderRadius.circular(16),
                                  boxShadow: [
                                    BoxShadow(
                                      color: const Color(0xFF0052FF).withOpacity(0.3),
                                      blurRadius: 10,
                                      offset: const Offset(0, 4),
                                    ),
                                  ],
                                ),
                                alignment: Alignment.center,
                                child: const Icon(
                                  Icons.mark_email_unread,
                                  color: Colors.white,
                                  size: 24,
                                ),
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 32),
              // Typography
              Text(
                'Check your inbox',
                style: GoogleFonts.plusJakartaSans(
                  fontSize: 24,
                  fontWeight: FontWeight.bold,
                  color: const Color(0xFF0A192F),
                ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 12),
              RichText(
                textAlign: TextAlign.center,
                text: TextSpan(
                  style: GoogleFonts.plusJakartaSans(
                    fontSize: 16,
                    color: const Color(0xFF434656),
                    height: 1.5,
                  ),
                  children: [
                    const TextSpan(text: 'We’ve sent a verification link to '),
                    TextSpan(
                      text: displayEmail,
                      style: const TextStyle(
                        color: Color(0xFF003EC7),
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    const TextSpan(text: '.\nPlease tap the link to activate your account.'),
                  ],
                ),
              ),
              const Spacer(),
              // Actions Section
              ElevatedButton(
                onPressed: () {
                  // Open Email App Logic
                },
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF003EC7),
                  foregroundColor: Colors.white,
                  elevation: 4,
                  shadowColor: const Color(0xFF003EC7).withOpacity(0.4),
                  minimumSize: const Size.fromHeight(56),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(28),
                  ),
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Icon(Icons.mail, size: 20),
                    const SizedBox(width: 8),
                    Text(
                      'Open Email App',
                      style: GoogleFonts.plusJakartaSans(
                        fontSize: 16,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 12),
              TextButton(
                onPressed: _onResendEmail,
                style: TextButton.styleFrom(
                  minimumSize: const Size.fromHeight(48),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                child: _isResent
                    ? Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          const Icon(Icons.check_circle, color: Color(0xFF10B981), size: 20),
                          const SizedBox(width: 8),
                          Text(
                            'Email Sent',
                            style: GoogleFonts.plusJakartaSans(
                              color: const Color(0xFF10B981),
                              fontSize: 14,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ],
                      )
                    : Text(
                        'Resend Email',
                        style: GoogleFonts.plusJakartaSans(
                          color: const Color(0xFF003EC7),
                          fontSize: 14,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
              ),
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 8),
                child: Row(
                  children: [
                    Expanded(child: Divider(color: const Color(0xFFC3C5D9).withOpacity(0.3))),
                    Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 12),
                      child: Text(
                        'or',
                        style: GoogleFonts.plusJakartaSans(
                          color: const Color(0xFF737688),
                          fontSize: 12,
                        ),
                      ),
                    ),
                    Expanded(child: Divider(color: const Color(0xFFC3C5D9).withOpacity(0.3))),
                  ],
                ),
              ),
              TextButton(
                onPressed: () => context.pop(),
                style: TextButton.styleFrom(
                  minimumSize: const Size.fromHeight(48),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                child: Text(
                  'Change Email Address',
                  style: GoogleFonts.plusJakartaSans(
                    color: const Color(0xFF515F78),
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
              const SizedBox(height: 32),
              // Footer Support
              Text.rich(
                TextSpan(
                  text: 'Didn\'t receive anything? Check your spam folder or contact ',
                  style: GoogleFonts.plusJakartaSans(
                    color: const Color(0xFF737688),
                    fontSize: 12,
                  ),
                  children: [
                    TextSpan(
                      text: 'Support',
                      style: GoogleFonts.plusJakartaSans(
                        color: const Color(0xFF003EC7),
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    const TextSpan(text: '.'),
                  ],
                ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 24),
            ],
          ),
        ),
      ),
    );
  }
}
