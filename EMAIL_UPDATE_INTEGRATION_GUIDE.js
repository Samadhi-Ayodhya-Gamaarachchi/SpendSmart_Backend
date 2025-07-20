/* 
 * Frontend Integration Guide for Email Update Feature
 * =====================================================
 * 
 * This guide shows how to integrate the email update functionality 
 * into your user settings page on the frontend.
 */

// Example API calls for email update functionality

// 1. Check if user has pending email change
const checkPendingEmailChange = async () => {
    try {
        const response = await fetch('http://localhost:5110/api/EmailVerification/pending-change', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${userToken}`, // Add JWT token here
                'Content-Type': 'application/json'
            }
        });
        
        const data = await response.json();
        
        if (data.success && data.hasPendingChange) {
            // Show user that email change is pending
            console.log(`Email change pending: ${data.pendingEmail}`);
            console.log(`Expires at: ${data.expiresAt}`);
            // Display UI to cancel pending change
        }
        
        return data;
    } catch (error) {
        console.error('Error checking pending email change:', error);
    }
};

// 2. Request email change (when user clicks "Update" button)
const requestEmailChange = async (newEmail) => {
    try {
        const response = await fetch('http://localhost:5110/api/EmailVerification/request-change', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${userToken}`, // Add JWT token here
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                newEmail: newEmail
            })
        });
        
        const data = await response.json();
        
        if (data.success) {
            // Show success message
            alert('Verification email sent! Please check your new email inbox.');
        } else {
            // Show error message
            alert(data.message);
        }
        
        return data;
    } catch (error) {
        console.error('Error requesting email change:', error);
        alert('An error occurred. Please try again.');
    }
};

// 3. Cancel pending email change
const cancelEmailChange = async () => {
    try {
        const response = await fetch('http://localhost:5110/api/EmailVerification/cancel-change', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${userToken}`, // Add JWT token here
                'Content-Type': 'application/json'
            }
        });
        
        const data = await response.json();
        
        if (data.success) {
            alert('Email change request cancelled.');
            // Refresh the UI
        } else {
            alert(data.message);
        }
        
        return data;
    } catch (error) {
        console.error('Error cancelling email change:', error);
    }
};

// Example React component integration
const EmailUpdateSection = () => {
    const [newEmail, setNewEmail] = useState('');
    const [pendingChange, setPendingChange] = useState(null);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        // Check for pending email change on component mount
        checkPendingEmailChange().then(setPendingChange);
    }, []);

    const handleUpdateEmail = async (e) => {
        e.preventDefault();
        if (!newEmail) return;
        
        setLoading(true);
        const result = await requestEmailChange(newEmail);
        setLoading(false);
        
        if (result?.success) {
            setNewEmail('');
            // Refresh pending change status
            checkPendingEmailChange().then(setPendingChange);
        }
    };

    const handleCancelChange = async () => {
        const result = await cancelEmailChange();
        if (result?.success) {
            setPendingChange(null);
        }
    };

    return (
        <div className="email-update-section">
            <h3>Email Address</h3>
            
            {pendingChange?.hasPendingChange ? (
                <div className="pending-change">
                    <p>Current Email: {pendingChange.currentEmail}</p>
                    <p>Pending Email: {pendingChange.pendingEmail}</p>
                    <p>Please check your new email for verification link.</p>
                    <button onClick={handleCancelChange}>Cancel Email Change</button>
                </div>
            ) : (
                <form onSubmit={handleUpdateEmail}>
                    <input
                        type="email"
                        value={newEmail}
                        onChange={(e) => setNewEmail(e.target.value)}
                        placeholder="Enter your new email address"
                        required
                    />
                    <button type="submit" disabled={loading}>
                        {loading ? 'Sending...' : 'Update Email'}
                    </button>
                </form>
            )}
        </div>
    );
};

/* 
 * Backend API Endpoints Available:
 * ================================
 * 
 * 1. GET /api/EmailVerification/test
 *    - Test endpoint to verify controller is working
 * 
 * 2. POST /api/EmailVerification/request-change
 *    - Request email change with new email address
 *    - Body: { "newEmail": "newemail@example.com" }
 *    - Requires Authorization header with JWT token
 * 
 * 3. GET /api/EmailVerification/verify-change?userId=1&token=abc123
 *    - Email verification link (clicked from email)
 *    - Redirects to frontend with success/error status
 * 
 * 4. POST /api/EmailVerification/cancel-change
 *    - Cancel pending email change
 *    - Requires Authorization header with JWT token
 * 
 * 5. GET /api/EmailVerification/pending-change
 *    - Check if user has pending email change
 *    - Requires Authorization header with JWT token
 * 
 * Email Flow:
 * ===========
 * 1. User enters new email and clicks "Update"
 * 2. Backend validates email and sends verification email
 * 3. User receives email with verification link
 * 4. User clicks link → backend verifies and updates email
 * 5. User is redirected to settings page with success message
 * 
 * Database Columns Added to Users table:
 * ======================================
 * - PendingEmail (nvarchar(max), nullable) - stores new email during verification
 * - EmailChangeToken (nvarchar(max), nullable) - verification token
 * - EmailChangeTokenExpiry (datetime2, nullable) - token expiration (24 hours)
 */
