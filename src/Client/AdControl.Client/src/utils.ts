export function getWordByCount(count: number, wordForms: [string, string, string]): string {
  const lastDigit = count % 10;
  const lastTwoDigits = count % 100;

  if (lastTwoDigits >= 11 && lastTwoDigits <= 14) {
    return `${count} ${wordForms[2]}`;
  }

  switch (lastDigit) {
    case 1:
      return `${count} ${wordForms[0]}`;
    case 2:
    case 3:
    case 4:
      return `${count} ${wordForms[1]}`;
    default:
      return `${count} ${wordForms[2]}`;
  }
}

// Использование:
getWordByCount(1, ['объект', 'объекта', 'объектов']); // "1 объект"
getWordByCount(2, ['объект', 'объекта', 'объектов']); // "2 объекта"
getWordByCount(5, ['объект', 'объекта', 'объектов']); // "5 объектов"

export function joinResolutionData(width: string, height: string): string {
  return `${width}x${height}`;
}

export function truncateString(str: string, maxLength: number, ellipsis = "...") {
  if (typeof str !== 'string') {
    throw new TypeError('Первый аргумент должен быть строкой');
  }

  if (str.length <= maxLength) {
    return str;
  }

  return str.slice(0, maxLength - ellipsis.length) + ellipsis;
}

export function formatDateShort(dateString?: string | null): string {
  if (!dateString) return "—";

  const date = new Date(dateString);

  if (isNaN(date.getTime())) {
    return "—";
  }

  return new Intl.DateTimeFormat("ru-RU", {
    day: "numeric",
    month: "short",
  }).format(date);
}

export function formatDateTimeShort(dateString?: string | null): string {
  if (!dateString) return "—";

  const date = new Date(dateString);

  if (isNaN(date.getTime())) {
    return "—";
  }

  // Вариант 1: Использовать отдельно дату и время
  return new Intl.DateTimeFormat("ru-RU", {
    day: "numeric",
    month: "short",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date);
}

export function buildMinioUrl(
    baseUrl: string,
    filePath?: string | null
): string | null {
  if (!filePath) return null;

  const cleanBase = baseUrl.replace(/\/$/, "");
  const cleanPath = filePath.startsWith("/")
      ? filePath.slice(1)
      : filePath;

  return `${cleanBase}/${encodeURIComponent(cleanPath)}`;
}

export function removeId(filename: string): string {
  const uuidRegex = /_[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i;
  return filename.replace(uuidRegex, '');
}

export function bytesToMB(bytes: number | undefined, decimals: number = 2): string {
  if (bytes === 0 || bytes === undefined) return '0 МБ';
  const mb = bytes / (1024 * 1024);
  const rounded = Math.round(mb * Math.pow(10, decimals)) / Math.pow(10, decimals);
  const formatted = parseFloat(rounded.toFixed(decimals)).toString();
  return `${formatted} МБ`;
}

export const generateRandomPassword = (length = 12): string => {
  const uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
  const lowercase = "abcdefghijklmnopqrstuvwxyz";
  const numbers = "0123456789";
  const special = "!@#$%^&*";

  const allChars = uppercase + lowercase + numbers + special;
  let password = "";

  password += uppercase[Math.floor(Math.random() * uppercase.length)];
  password += lowercase[Math.floor(Math.random() * lowercase.length)];
  password += numbers[Math.floor(Math.random() * numbers.length)];
  password += special[Math.floor(Math.random() * special.length)];

  for (let i = 4; i < length; i++) {
    password += allChars[Math.floor(Math.random() * allChars.length)];
  }

  return password.split('').sort(() => Math.random() - 0.5).join('');
};

export const isVideo = (name: string) =>
    ["mp4", "webm", "ogg"].some(ext => name.toLowerCase().endsWith(ext));

export const isImage = (name: string) =>
    ["jpg", "jpeg", "png", "gif", "webp"].some(ext =>
        name.toLowerCase().endsWith(ext)
    );
