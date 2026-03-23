// Function to format a number as currency
function formatCurrency(number, locale = 'vi-VN', currency = 'VND') {
    return new Intl.NumberFormat(locale, {
        style: 'currency',
        currency: currency,
    }).format(number);
}

function formatNumberCur(number, localeName = '') {
  
    // Chuyển đổi đầu vào thành một số
    if (localeName == 'zhhans') {
        return number.replaceAll(',', '');
    }
    else {
        var arr = number.split('.');
        if (arr.length > 1) {
            return (arr[0].replaceAll(',', '') + ',' + arr[1]);
        }
        else {
            return number;
        }
    }
    
}

function formatThousand(number) {
    // Chuyển đổi đầu vào thành một số
    const num = parseFloat(number);

    // Kiểm tra nếu đầu vào không phải là số hợp lệ thì trả về chuỗi rỗng
    if (isNaN(num)) {
        return '';
    }

    // Làm tròn số đến 2 chữ số thập phân
    const roundedNum = Math.round(num * 100) / 100;

    // Sử dụng `toLocaleString()` để định dạng số theo chuẩn Việt Nam
    // `minimumFractionDigits: 2` sẽ đảm bảo luôn có 2 chữ số sau dấu phẩy
    const formatted = roundedNum.toLocaleString('vi-VN', {
        minimumFractionDigits: num % 1 === 0 ? 0 : 2,
        maximumFractionDigits: 2
    });

    return formatted;
}



function formatNumberThousand(input, fractionDigits = 2) {
    // Chuyển đổi đầu vào thành một số
    const num = parseFloat(input);

    // Kiểm tra nếu đầu vào không phải là số hợp lệ thì trả về chuỗi rỗng
    if (isNaN(num)) {
        return '';
    }

    // Làm tròn số đến số chữ số thập phân mong muốn
    const roundedNum = Number(num.toFixed(fractionDigits));

    // Sử dụng `toLocaleString()` để định dạng số theo chuẩn Việt Nam
    // `minimumFractionDigits` và `maximumFractionDigits` sẽ đảm bảo số chữ số sau dấu phẩy
    const formatted = roundedNum.toLocaleString('vi-VN', {
        minimumFractionDigits: fractionDigits,
        maximumFractionDigits: fractionDigits
    });

    return formatted;
}




function convertToStandardNumber(formattedString) {
    // 1. Kiểm tra nếu đầu vào không phải là chuỗi, trả về chuỗi rỗng


    if (typeof formattedString !== 'string') {
        return '';
    }

    //// 2. Loại bỏ tất cả các dấu chấm (.) dùng cho phân cách hàng nghìn
    let standardFormat = formattedString.replace(/,/g, '');

    //// 3. Thay thế dấu phẩy (,) dùng cho phần thập phân bằng dấu chấm (.)
    //standardFormat = standardFormat.replace(/,/g, '.');

    //// Trả về chuỗi đã được chuyển đổi
    //return standardFormat;
    return standardFormat;
}

// Function to format a date to dd/MM/yyyy
Date.prototype.toDDMMYYYY = function () {
    const day = String(this.getDate()).padStart(2, '0');
    const month = String(this.getMonth() + 1).padStart(2, '0');
    const year = this.getFullYear();
    return `${day}/${month}/${year}`;
};

// Function to format a date to dd/MM/yyyy HH:mm
Date.prototype.toDDMMYYYYHHmm = function () {
    const day = String(this.getDate()).padStart(2, '0');
    const month = String(this.getMonth() + 1).padStart(2, '0');
    const year = this.getFullYear();
    const hours = String(this.getHours()).padStart(2, '0');
    const minutes = String(this.getMinutes()).padStart(2, '0');
    return `${day}/${month}/${year} ${hours}:${minutes}`;
};

// Example usage:
// console.log(new Date().toDDMMYYYY()); // "07/01/2025"
// console.log(new Date().toDDMMYYYYHHmm()); // "07/01/2025 14:30"
function formatDateToDDMMYYYY(date) {
    if (!(date instanceof Date)) {
        date = new Date(date);
    }
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
}

function formatDateToDDMMYYYYHHmm(date) {
    if (!(date instanceof Date)) {
        date = new Date(date);
    }
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${day}/${month}/${year} ${hours}:${minutes}`;
}

function formatDateToDDMMYYYYHHmmss(date) {
    if (!(date instanceof Date)) {
        date = new Date(date);
    }
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');
    return `${day}/${month}/${year} ${hours}:${minutes}:${seconds}`;
}

function PlayAudio(type, callback) {
    let audio;
    let timeout;
    if (type === 'success') {
        audio = new Audio('/sounds/success.mp3');
    } else if (type === 'warning') {
        audio = new Audio('/sounds/alert.mp3');
    }

    if (audio) {
        audio.play();
        timeout = setTimeout(() => {
            if (callback) callback('timeout');
        }, 3000); // Timeout sau 3 giây phòng trường hợp lỗi

        audio.onended = () => {
            clearTimeout(timeout);
            if (callback) callback('completed');
        };
    } else {
        if (callback) callback('no_audio');
    }
}
function PlaySound(type, callback) {
    let audio;
    let timeout;
    let isCallbackCalled = false; // Flag to prevent duplicate calls

    if (type === 'success') {
        audio = new Audio('/sounds/success.mp3');
    } else if (type === 'warning') {
        audio = new Audio('/sounds/alert.mp3');
    } else if (type === 'note') {
        audio = new Audio('/sounds/note.wav');
    }
    else if (type === '007') {
        audio = new Audio('/sounds/007.wav');
    }
    else if (type === 'export') {
        audio = new Audio('/sounds/export.mp3');
    }
    
    if (audio) {
        audio.play();

        timeout = setTimeout(() => {
            if (!isCallbackCalled) {
                isCallbackCalled = true;
                if (callback) callback('timeout');
            }
        }, 1000);

        audio.onended = () => {
            if (!isCallbackCalled) {
                clearTimeout(timeout);
                isCallbackCalled = true;
                if (callback) callback('completed');
            }
        };
    } else {
        if (callback) callback('no_audio');
    }
}

function toQueryString(obj) {
    if (!obj) return '';
    return Object.keys(obj)
        .map(key => {
            const value = obj[key];
            if (value === undefined || value === null) return '';
            if (Array.isArray(value)) {
                return value
                    .map(v => encodeURIComponent(key) + '=' + encodeURIComponent(v))
                    .join('&');
            }
            return encodeURIComponent(key) + '=' + encodeURIComponent(value);
        })
        .filter(x => x.length > 0)
        .join('&');
}

// Example usage:
// console.log(formatCurrency(123456789)); // "123.456.789 đ"
// console.log(formatDateToDDMMYYYY(new Date())); // "07/01/2025"
function FormatNumberToUpload(formattedString) {
    // Chuyển đổi đầu vào thành số
    // 1. Kiểm tra nếu đầu vào không phải là chuỗi, trả về chuỗi rỗng
     
    if (typeof formattedString !== 'string') {
        return 0;
    }

    //// 2. Loại bỏ tất cả các dấu chấm (.) dùng cho phân cách hàng nghìn
    let standardFormat = formattedString;

    //// 3. Thay thế dấu phẩy (,) dùng cho phần thập phân bằng dấu chấm (.)
    standardFormat = standardFormat.replaceAll(/\./g, '');
    standardFormat = standardFormat.replaceAll(/,/g, '.');

    //// Trả về chuỗi đã được chuyển đổi
    //return standardFormat;
    return standardFormat;
}

function FormatNumberToDisplay(str, n = 0) {

    // Kiểm tra đầu vào
    if (str === null || str === undefined || isNaN(parseFloat(str))) {
        return '0';
    }
    // Chuyển đổi sang số và làm tròn đến n chữ số thập phân
    const num = Number(parseFloat(str).toFixed(n));
    // Định dạng số theo kiểu 123,456.78
    return num.toLocaleString('en-US', {
        minimumFractionDigits: n,
        maximumFractionDigits: n
    });
}


