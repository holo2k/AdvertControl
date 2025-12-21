import ContentLoader from "react-content-loader";

export const TableLoader = () => (
    <ContentLoader
        speed={2}
        width="100%"
        height={520}
        viewBox="0 0 1200 520"
        backgroundColor="#f3f3f3"
        foregroundColor="#ecebeb"
        className="w-full"
    >
        {/* Заголовки */}
        <rect x="5%"  y="12"  rx="4" ry="4" width="20%" height="48" />
        <rect x="28%" y="12"  rx="4" ry="4" width="22%" height="48" />
        <rect x="52%" y="12"  rx="4" ry="4" width="15%" height="48" />
        <rect x="68%" y="12"  rx="4" ry="4" width="18%" height="48" />
        <rect x="88%" y="12"  rx="4" ry="4" width="10%" height="48" />

        {/* Строка 1 */}
        <rect x="5%"  y="80"  rx="4" ry="4" width="22%" height="40" />
        <rect x="28%" y="80"  rx="4" ry="4" width="25%" height="40" />
        <rect x="52%" y="80"  rx="4" ry="4" width="12%" height="40" />
        <rect x="68%" y="76"  rx="12" ry="12" width="80" height="40" />
        <rect x="88%" y="76"  rx="6"  ry="6"  width="60" height="40" />

        {/* Строка 2 */}
        <rect x="5%"  y="140" rx="4" ry="4" width="22%" height="40" />
        <rect x="28%" y="140" rx="4" ry="4" width="25%" height="40" />
        <rect x="52%" y="140" rx="4" ry="4" width="12%" height="40" />
        <rect x="68%" y="136" rx="12" ry="12" width="80" height="40" />
        <rect x="88%" y="136" rx="6"  ry="6"  width="60" height="40" />

        <rect x="5%"  y="200" rx="4" ry="4" width="22%" height="40" />
        <rect x="28%" y="200" rx="4" ry="4" width="25%" height="40" />
        <rect x="52%" y="200" rx="4" ry="4" width="12%" height="40" />
        <rect x="68%" y="196" rx="12" ry="12" width="80" height="40" />
        <rect x="88%" y="196" rx="6"  ry="6"  width="60" height="40" />

        <rect x="5%"  y="260" rx="4" ry="4" width="22%" height="40" />
        <rect x="28%" y="260" rx="4" ry="4" width="25%" height="40" />
        <rect x="52%" y="260" rx="4" ry="4" width="12%" height="40" />
        <rect x="68%" y="256" rx="12" ry="12" width="80" height="40" />
        <rect x="88%" y="256" rx="6"  ry="6"  width="60" height="40" />
    </ContentLoader>
);
