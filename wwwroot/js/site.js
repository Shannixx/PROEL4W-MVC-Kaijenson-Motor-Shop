/* ============================================================
   KAIJENSON MOTOR SHOP — Global JavaScript Utilities
   ============================================================ */

// ── Toast Notification System ──
(function () {
    let toastContainer = null;

    function ensureContainer() {
        if (!toastContainer) {
            toastContainer = document.getElementById('toastContainer');
            if (!toastContainer) {
                toastContainer = document.createElement('div');
                toastContainer.id = 'toastContainer';
                toastContainer.className = 'toast-container';
                document.body.appendChild(toastContainer);
            }
        }
        return toastContainer;
    }

    window.showToast = function (message, type) {
        type = type || 'success';
        const container = ensureContainer();
        const toast = document.createElement('div');
        toast.className = 'toast toast-' + type;

        const icons = {
            success: 'fa-check-circle',
            error: 'fa-exclamation-circle',
            info: 'fa-info-circle',
            warning: 'fa-exclamation-triangle'
        };

        toast.innerHTML =
            '<i class="fas ' + (icons[type] || icons.info) + '"></i>' +
            '<span>' + message + '</span>' +
            '<button class="toast-close" onclick="this.parentElement.remove()"><i class="fas fa-xmark"></i></button>';

        container.appendChild(toast);

        // Auto-remove after 4 seconds
        setTimeout(function () {
            if (toast.parentElement) {
                toast.style.animation = 'toastSlideOut 0.3s ease forwards';
                setTimeout(function () { toast.remove(); }, 300);
            }
        }, 4000);
    };
})();

// ── Export to Excel (requires SheetJS/xlsx) ──
window.exportToExcel = function (data, sheetName, fileName, colWidths) {
    if (typeof XLSX === 'undefined') {
        showToast('Excel library not loaded', 'error');
        return;
    }
    var ws = XLSX.utils.json_to_sheet(data);
    if (colWidths) {
        ws['!cols'] = colWidths.map(function (w) { return { wch: w }; });
    }
    var wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, sheetName || 'Sheet1');
    XLSX.writeFile(wb, fileName || 'Export.xlsx');
    showToast(sheetName + ' exported as Excel');
};

// Multi-sheet export
window.exportToExcelMulti = function (sheets, fileName) {
    if (typeof XLSX === 'undefined') {
        showToast('Excel library not loaded', 'error');
        return;
    }
    var wb = XLSX.utils.book_new();
    sheets.forEach(function (s) {
        var ws = XLSX.utils.json_to_sheet(s.data);
        if (s.colWidths) {
            ws['!cols'] = s.colWidths.map(function (w) { return { wch: w }; });
        }
        XLSX.utils.book_append_sheet(wb, ws, s.name);
    });
    XLSX.writeFile(wb, fileName || 'Export.xlsx');
    showToast('Report exported as Excel');
};

// ── Bulk Select Helpers ──
window.BulkSelect = {
    selectedIds: new Set(),

    toggle: function (id, checkbox) {
        if (checkbox.checked) {
            this.selectedIds.add(id);
        } else {
            this.selectedIds.delete(id);
        }
        this.updateUI();
    },

    toggleAll: function (headerCheckbox, selector) {
        var checkboxes = document.querySelectorAll(selector || '.row-checkbox');
        var self = this;
        checkboxes.forEach(function (cb) {
            cb.checked = headerCheckbox.checked;
            var id = cb.getAttribute('data-id');
            if (headerCheckbox.checked) {
                self.selectedIds.add(id);
            } else {
                self.selectedIds.delete(id);
            }
        });
        this.updateUI();
    },

    selectAll: function (ids) {
        var self = this;
        ids.forEach(function (id) { self.selectedIds.add(String(id)); });
        document.querySelectorAll('.row-checkbox').forEach(function (cb) {
            cb.checked = true;
        });
        var headerCb = document.querySelector('.header-checkbox');
        if (headerCb) headerCb.checked = true;
        this.updateUI();
    },

    deselectAll: function () {
        this.selectedIds.clear();
        document.querySelectorAll('.row-checkbox').forEach(function (cb) {
            cb.checked = false;
        });
        var headerCb = document.querySelector('.header-checkbox');
        if (headerCb) headerCb.checked = false;
        this.updateUI();
    },

    updateUI: function () {
        var bar = document.getElementById('bulkActionBar');
        var countEl = document.getElementById('selectedCount');
        var totalEl = document.getElementById('filteredCount');
        var deleteCountEl = document.getElementById('deleteSelectedCount');

        if (bar) {
            bar.style.display = this.selectedIds.size > 0 ? 'flex' : 'none';
        }
        if (countEl) countEl.textContent = this.selectedIds.size;
        if (deleteCountEl) deleteCountEl.textContent = this.selectedIds.size;

        // Highlight selected rows
        document.querySelectorAll('.row-checkbox').forEach(function (cb) {
            var row = cb.closest('tr');
            if (row) {
                row.classList.toggle('row-selected', cb.checked);
            }
        });
    },

    getIds: function () {
        return Array.from(this.selectedIds);
    },

    reset: function () {
        this.selectedIds.clear();
        this.updateUI();
    }
};

// ── Modal Helpers ──
window.openModal = function (modalId) {
    var modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'flex';
        document.body.style.overflow = 'hidden';
    }
};

window.closeModal = function (modalId) {
    var modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'none';
        document.body.style.overflow = '';
    }
};

// Close modal on overlay click
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('modal-overlay')) {
        e.target.style.display = 'none';
        document.body.style.overflow = '';
    }
});

// ── Print Receipt ──
window.printReceipt = function (orderData) {
    var itemsHTML = '';
    if (orderData.items && orderData.items.length) {
        orderData.items.forEach(function (item) {
            var subtotal = (item.price * item.quantity);
            itemsHTML += '<tr>' +
                '<td style="padding:4px 0;font-size:12px;">' + item.productName + '</td>' +
                '<td style="padding:4px 0;font-size:12px;text-align:center;">' + item.quantity + '</td>' +
                '<td style="padding:4px 0;font-size:12px;text-align:right;">₱' + subtotal.toLocaleString('en-PH',{minimumFractionDigits:2}) + '</td>' +
                '</tr>';
        });
    }

    var printWindow = window.open('', '_blank', 'width=400,height=600');
    if (!printWindow) {
        showToast('Please allow popups for printing', 'error');
        return;
    }

    printWindow.document.write('<!DOCTYPE html>' +
        '<html><head><title>Order ' + orderData.id + '</title>' +
        '<style>body{font-family:"Courier New",monospace;padding:20px;max-width:300px;margin:0 auto;color:#333}' +
        '.center{text-align:center}.divider{border-top:1px dashed #ccc;margin:10px 0}table{width:100%;border-collapse:collapse}' +
        '@media print{body{margin:0;padding:10px}}</style></head><body>' +
        '<div class="center"><h2 style="margin:0;font-size:16px;">Kaijenson Motor Shop</h2>' +
        '<p style="margin:4px 0;font-size:11px;color:#666;">Order Receipt</p></div>' +
        '<div class="divider"></div>' +
        '<p style="font-size:12px;margin:4px 0;"><strong>Order:</strong> #' + orderData.id + '</p>' +
        '<p style="font-size:12px;margin:4px 0;"><strong>Date:</strong> ' + orderData.date + '</p>' +
        '<p style="font-size:12px;margin:4px 0;"><strong>Customer:</strong> ' + orderData.customer + '</p>' +
        '<p style="font-size:12px;margin:4px 0;"><strong>Payment:</strong> ' + orderData.payment + '</p>' +
        '<p style="font-size:12px;margin:4px 0;"><strong>Status:</strong> ' + orderData.status + '</p>' +
        '<div class="divider"></div>' +
        '<table><thead><tr style="font-size:11px;color:#666;">' +
        '<th style="text-align:left;padding-bottom:6px;">Item</th>' +
        '<th style="text-align:center;padding-bottom:6px;">Qty</th>' +
        '<th style="text-align:right;padding-bottom:6px;">Amount</th></tr></thead>' +
        '<tbody>' + itemsHTML + '</tbody></table>' +
        '<div class="divider"></div>' +
        '<table><tr style="font-weight:bold;font-size:14px;"><td>TOTAL</td>' +
        '<td style="text-align:right;">₱' + parseFloat(orderData.total).toLocaleString('en-PH',{minimumFractionDigits:2}) + '</td></tr></table>' +
        '<div class="divider"></div>' +
        '<div class="center"><p style="font-size:11px;color:#666;">Thank you for your purchase!</p></div>' +
        '</body></html>');

    printWindow.document.close();
    printWindow.focus();
    setTimeout(function () { printWindow.print(); }, 300);
};

// ── AJAX Helper ──
window.postJSON = function (url, data) {
    var token = document.querySelector('input[name="__RequestVerificationToken"]');
    return fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token ? token.value : ''
        },
        body: JSON.stringify(data)
    }).then(function (r) { return r.json(); });
};

// ── Format Helpers ──
window.formatPeso = function (amount) {
    return '₱' + parseFloat(amount).toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
};

window.formatNumber = function (num) {
    return parseFloat(num).toLocaleString('en-PH');
};

// ── Confirmation Dialog ──
window.confirmAction = function (message, callback) {
    if (confirm(message)) {
        callback();
    }
};

// ── Date Helpers ──
window.todayISO = function () {
    return new Date().toISOString().split('T')[0];
};
