import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/shared/app_snackbar.dart';
import 'providers/auth_controller.dart';
import '../../../core/state/base_state.dart';
import '../data/models/auth_response.dart';

class ForgotPasswordScreen extends ConsumerStatefulWidget {
  const ForgotPasswordScreen({super.key});

  @override
  ConsumerState<ForgotPasswordScreen> createState() => _ForgotPasswordScreenState();
}

class _ForgotPasswordScreenState extends ConsumerState<ForgotPasswordScreen> with SingleTickerProviderStateMixin {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  bool _isSuccess = false;
  late AnimationController _animationController;
  late Animation<double> _floatingAnimation;

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
    _emailController.dispose();
    _animationController.dispose();
    super.dispose();
  }

  void _onSubmit() {
    if (_formKey.currentState!.validate()) {
      ref.read(authControllerProvider.notifier).forgotPassword(
            _emailController.text.trim(),
          );
    }
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authControllerProvider);
    final isLoading = authState.maybeWhen(
      loading: () => true,
      orElse: () => false,
    );

    ref.listen<BaseState<AuthResponseModel>>(authControllerProvider, (previous, next) {
      next.maybeWhen(
        error: (error) {
          AppSnackbar.showError(context, error.toString());
        },
        empty: () {
          setState(() {
            _isSuccess = true;
          });
        },
        orElse: () {},
      );
    });

    return Scaffold(
      backgroundColor: const Color(0xFFF7F9FB),
      body: Stack(
        children: [
          // Animated Background Ornament
          Positioned(
            top: -96,
            right: -96,
            child: Container(
              width: 256,
              height: 256,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: const Color(0xFF0052FF).withOpacity(0.1),
              ),
            ),
          ),
          Positioned(
            top: MediaQuery.of(context).size.height / 2 - 160,
            left: -128,
            child: Container(
              width: 320,
              height: 320,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: const Color(0xFF6366F1).withOpacity(0.1),
              ),
            ),
          ),
          // Blur effect
          Positioned.fill(
            child: BackdropFilter(
              filter: ImageFilter.blur(sigmaX: 100, sigmaY: 100),
              child: const SizedBox(),
            ),
          ),
          
          // Branding / Atmospheric Illustration (Background)
          if (!_isSuccess)
            Positioned(
              bottom: 24,
              left: 0,
              right: 0,
              child: Opacity(
                opacity: 0.2,
                child: Image.network(
                  'https://lh3.googleusercontent.com/aida-public/AB6AXuCEyui1LBpLrkxrT1kQiUovSDwV8kMUDIT7YjgLJimiq8NFM9l0DqXETOfoupPqZQhM_nvACtFOcwf2c_77vDMeQ1nQUHJMi-1JDMq0lClaVpdx03-Wy33188xnsv1zsoYETbnuGgFReZvG8IDZiJ2E1PXhMaDzwIPMKJlq2n2qrmIPv2C3BKcxzq8sm6fAPbRwf-sdKDTPS1ssb0Hqw9LlGzcboKd8AA9BswDOFRpBBxnAGQnpDTir',
                  height: 250,
                  fit: BoxFit.contain,
                ),
              ),
            ),

          SafeArea(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Top App Bar
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
                  child: TextButton.icon(
                    onPressed: () => context.pop(),
                    icon: const Icon(Icons.arrow_back_ios_new, color: Color(0xFF003EC7), size: 18),
                    label: Text(
                      'Back to Login',
                      style: GoogleFonts.plusJakartaSans(
                        color: const Color(0xFF003EC7),
                        fontSize: 14,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    style: TextButton.styleFrom(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                      minimumSize: Size.zero,
                      tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                    ),
                  ),
                ),
                
                Expanded(
                  child: SingleChildScrollView(
                    padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
                    child: _isSuccess ? _buildSuccessState() : _buildFormState(isLoading),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFormState(bool isLoading) {
    return Form(
      key: _formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const SizedBox(height: 16),
          // Hero Section
          Align(
            alignment: Alignment.centerLeft,
            child: AnimatedBuilder(
              animation: _floatingAnimation,
              builder: (context, child) {
                return Transform.translate(
                  offset: Offset(0, _floatingAnimation.value),
                  child: child,
                );
              },
              child: Container(
                width: 64,
                height: 64,
                decoration: BoxDecoration(
                  color: const Color(0xFFDDE1FF),
                  borderRadius: BorderRadius.circular(16),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withOpacity(0.05),
                      blurRadius: 4,
                    ),
                  ],
                ),
                alignment: Alignment.center,
                child: const Icon(Icons.lock_reset, color: Color(0xFF003EC7), size: 36),
              ),
            ),
          ),
          const SizedBox(height: 24),
          Text(
            'Forgot Password?',
            style: GoogleFonts.plusJakartaSans(
              fontSize: 24,
              fontWeight: FontWeight.bold,
              color: const Color(0xFF0A192F),
            ),
          ),
          const SizedBox(height: 8),
          Text(
            'Enter the email address associated with your AISAM account and we\'ll send you a link to reset your password.',
            style: GoogleFonts.plusJakartaSans(
              fontSize: 16,
              color: const Color(0xFF515F78),
            ),
          ),
          const SizedBox(height: 32),
          // Form Section
          Padding(
            padding: const EdgeInsets.only(left: 4, bottom: 8),
            child: Text(
              'Email Address',
              style: GoogleFonts.plusJakartaSans(
                fontSize: 14,
                fontWeight: FontWeight.w600,
                color: const Color(0xFF434656),
              ),
            ),
          ),
          TextFormField(
            controller: _emailController,
            keyboardType: TextInputType.emailAddress,
            decoration: InputDecoration(
              hintText: 'e.g. creative@aisam.com',
              hintStyle: GoogleFonts.plusJakartaSans(color: const Color(0xFFC3C5D9)),
              prefixIcon: const Icon(Icons.mail, color: Color(0xFF737688), size: 20),
              filled: true,
              fillColor: const Color(0xFFF2F4F6),
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
                borderSide: BorderSide.none,
              ),
              focusedBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
                borderSide: const BorderSide(color: Color(0xFF003EC7), width: 2),
              ),
            ),
            style: GoogleFonts.plusJakartaSans(color: const Color(0xFF191C1E)),
            validator: (value) {
              if (value == null || value.isEmpty) return 'Please enter email';
              if (!value.contains('@')) return 'Invalid email';
              return null;
            },
          ),
          const SizedBox(height: 24),
          ElevatedButton(
            onPressed: isLoading ? null : _onSubmit,
            style: ElevatedButton.styleFrom(
              backgroundColor: const Color(0xFF003EC7),
              foregroundColor: Colors.white,
              minimumSize: const Size.fromHeight(56),
              elevation: 4,
              shadowColor: const Color(0xFF003EC7).withOpacity(0.4),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(28),
              ),
            ),
            child: isLoading
                ? const SizedBox(
                    width: 24,
                    height: 24,
                    child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2),
                  )
                : Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text(
                        'Send Recovery Link',
                        style: GoogleFonts.plusJakartaSans(
                          fontSize: 18,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                      const SizedBox(width: 8),
                      const Icon(Icons.send, size: 20),
                    ],
                  ),
          ),
          const SizedBox(height: 32),
          // Secondary CTA
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text(
                'Remembered it? ',
                style: GoogleFonts.plusJakartaSans(
                  fontSize: 14,
                  color: const Color(0xFF434656),
                ),
              ),
              TextButton(
                onPressed: () => context.pop(),
                style: TextButton.styleFrom(
                  padding: EdgeInsets.zero,
                  minimumSize: Size.zero,
                  tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                ),
                child: Text(
                  'Log in now',
                  style: GoogleFonts.plusJakartaSans(
                    fontSize: 14,
                    fontWeight: FontWeight.bold,
                    color: const Color(0xFF003EC7),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 150), // Spacing for illustration
        ],
      ),
    );
  }

  Widget _buildSuccessState() {
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        const SizedBox(height: 48),
        Container(
          width: 80,
          height: 80,
          decoration: BoxDecoration(
            color: const Color(0xFF10B981).withOpacity(0.1),
            shape: BoxShape.circle,
          ),
          alignment: Alignment.center,
          child: const Icon(Icons.check_circle, color: Color(0xFF10B981), size: 48),
        ),
        const SizedBox(height: 24),
        Text(
          'Check your inbox',
          style: GoogleFonts.plusJakartaSans(
            fontSize: 20,
            fontWeight: FontWeight.w600,
            color: const Color(0xFF0A192F),
          ),
        ),
        const SizedBox(height: 8),
        Text(
          'We\'ve sent a password recovery link to your email address. Please follow the instructions.',
          style: GoogleFonts.plusJakartaSans(
            fontSize: 16,
            color: const Color(0xFF515F78),
          ),
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 32),
        Container(
          padding: const EdgeInsets.all(24),
          decoration: BoxDecoration(
            color: const Color(0xFFF2F4F6),
            borderRadius: BorderRadius.circular(16),
            border: Border.all(color: const Color(0xFFC3C5D9).withOpacity(0.3)),
          ),
          child: Column(
            children: [
              Text(
                'Didn\'t receive the email?',
                style: GoogleFonts.plusJakartaSans(
                  fontSize: 12,
                  color: const Color(0xFF515F78),
                  fontStyle: FontStyle.italic,
                ),
              ),
              const SizedBox(height: 12),
              TextButton(
                onPressed: () {
                  // Revert back to form to send again
                  setState(() {
                    _isSuccess = false;
                  });
                },
                style: TextButton.styleFrom(
                  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                  backgroundColor: const Color(0xFF0052FF).withOpacity(0.1),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(24),
                  ),
                ),
                child: Text(
                  'Resend link',
                  style: GoogleFonts.plusJakartaSans(
                    fontSize: 14,
                    fontWeight: FontWeight.bold,
                    color: const Color(0xFF003EC7),
                  ),
                ),
              ),
            ],
          ),
        ),
        const SizedBox(height: 32),
        TextButton.icon(
          onPressed: () => context.pop(),
          icon: const Icon(Icons.west, size: 16, color: Color(0xFF434656)),
          label: Text(
            'Back to Login',
            style: GoogleFonts.plusJakartaSans(
              fontSize: 14,
              color: const Color(0xFF434656),
            ),
          ),
        ),
      ],
    );
  }
}
