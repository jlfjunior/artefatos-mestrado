const Logo = () => (
    <svg
        width="40"
        height="40"
        viewBox="0 0 200 200"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
    >
        <rect width="200" height="200" rx="20" fill="#2C3E50" />
        <rect x="50" y="70" width="100" height="60" rx="10" fill="#27AE60" />
        <circle cx="100" cy="100" r="15" fill="#ECF0F1" />
        <text x="93" y="107" fontFamily="Arial" fontSize="16" fontWeight="bold" fill="#2C3E50">$</text>
        <path
            d="M40 110 C20 100, 20 60, 50 50 L40 40 M40 40 L20 55"
            stroke="#F1C40F"
            strokeWidth="6"
            strokeLinecap="round"
            strokeLinejoin="round"
            fill="none"
        />
        <path
            d="M160 90 C180 100, 180 140, 150 150 L160 160 M160 160 L180 145"
            stroke="#F1C40F"
            strokeWidth="6"
            strokeLinecap="round"
            strokeLinejoin="round"
            fill="none"
        />
    </svg>
);

export default Logo;
