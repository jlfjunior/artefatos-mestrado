/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts,js}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#e6f1ff',
          100: '#cce3ff',
          200: '#99c7ff',
          300: '#2675ff',
          400: '#236cec',
          500: '#1e5bc6',
          600: '#005cb8',
          700: '#164393',
          800: '#123779',
          900: '#00172e',
        },
        secondary: {
          50: '#e6faff',
          100: '#ccf5ff',
          200: '#99ebff',
          300: '#66e0ff',
          400: '#33d6ff',
          500: '#00ccff',
          600: '#00a3cc',
          700: '#007a99',
          800: '#005266',
          900: '#002933',
        },
        neutral: {
          50: '#f8fafc',
          100: '#f1f5f9',
          200: '#e2e8f0',
          300: '#cbd5e1',
          400: '#94a3b8',
          500: '#64748b',
          600: '#475569',
          700: '#334155',
          800: '#1e293b',
          900: '#0f172a',
          950: '#020617',
        },
        success: {
          500: '#10b981',
        },
        warning: {
          500: '#f59e0b',
        },
        danger: {
          500: '#ef4444',
        }
      },
      fontFamily: {
        sans: ['Poppins', 'system-ui', 'sans-serif'],
      },
      spacing: {
        '0.5': '0.125rem',
        '1.5': '0.375rem',
        '2.5': '0.625rem',
        '3.5': '0.875rem',
      },
      boxShadow: {
        soft: '0 2px 10px rgba(0, 0, 0, 0.05)',
        card: '0 4px 20px rgba(0, 0, 0, 0.08)',
      }
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
  ],
}

